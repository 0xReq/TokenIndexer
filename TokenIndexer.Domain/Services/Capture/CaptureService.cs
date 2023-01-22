using System.Text.Json;
using Microsoft.Extensions.Logging;
using TokenIndexer.Domain.Clients;
using TokenIndexer.Domain.Constants;
using TokenIndexer.Domain.Entities;
using TokenIndexer.Domain.Entities.Enums;
using TokenIndexer.Domain.Helpers;
using TokenIndexer.Domain.Responses.NFT;
using TokenIndexer.Domain.Responses.RPC;
using TokenIndexer.Domain.Services.Snapshot;
using TokenIndexer.Domain.Services.Wallet;

namespace TokenIndexer.Domain.Services.Capture;

public class CaptureService : ICaptureService
{
    private readonly ILogger<CaptureService> _logger;
    private readonly ISnapshotService _snapshotService;
    private readonly IWalletService _walletService;
    private readonly IRpcClient _rpcClient;
    private readonly INftApiClient _nftApiClient;
    private const string TokenProgramId = "TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA";
    private List<string> _trackedCoins;

    public CaptureService(ILogger<CaptureService> logger, ISnapshotService snapshotService, IWalletService walletService,
        IRpcClient rpcClient, INftApiClient nftApiClient)
    {
        _logger = logger;
        _snapshotService = snapshotService;
        _walletService = walletService;
        _rpcClient = rpcClient;
        _nftApiClient = nftApiClient;
        _trackedCoins = new List<string>
        {
            TokenConstants.UsdcMint,
            TokenConstants.MsolMint,
            TokenConstants.BonkMint
        };
    }
    
    public async Task ProcessMessage(IEnumerable<string> wallets, EventType eventType)
    {
        foreach (var wallet in wallets)
        {
            try
            {
                var (lamports, tokenResult) = await GetTokensInWallet(wallet);

                var currentTokens = ValidateTokens(tokenResult);

                await GetMetadataAndEnrichTokens(currentTokens);

                await UpdateDelta(wallet, currentTokens);

                var snapshotDto = new SnapshotDto(wallet, lamports)
                {
                    Tokens = currentTokens
                };
                
                switch (eventType)
                {
                    case EventType.DailySnapshot:
                        await _snapshotService.CreateSnapshot(snapshotDto);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ProcessMessage)} Failed taking snapshot for wallet: {wallet}");
                throw;
            }
        }
    }

    private async Task UpdateDelta(string wallet, IEnumerable<TokenModel> currentTokens)
    {
        var getPreviousTokens = await _snapshotService.GetTokens(wallet, 1, 1);

        var delta = GetPercentageDelta(getPreviousTokens.SelectMany(_ => _.Tokens), currentTokens);

        await _walletService.Update(wallet, delta);
    }

    public decimal GetPercentageDelta(IEnumerable<TokenModel> previousTokens, IEnumerable<TokenModel> currentTokens)
    {
        try
        {
            var previous = (decimal)previousTokens.Count();
            var current = (decimal)currentTokens.Count();

            if (previous == 0 || current == 0) return 0;
        
            var increase = current - previous;
            var percentageDelta = (increase / previous) * 100;

            var roundedDelta = decimal.Round(percentageDelta, 2,  MidpointRounding.AwayFromZero);

            if (roundedDelta < 0)
                roundedDelta = Math.Abs(roundedDelta);
        
            return roundedDelta;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"{nameof(GetPercentageDelta)} Failed calculating delta");
            return 0;
        }
    }

    private async Task<(ulong, TokenResult)> GetTokensInWallet(string wallet)
    {
        var getBalanceTask = _rpcClient.GetBalance(wallet);
        var getTokensTask = _rpcClient.GetTokensByOwner(wallet, TokenProgramId);

        await Task.WhenAll(getBalanceTask, getTokensTask).ConfigureAwait(false);

        var balanceResult = getBalanceTask.Result;
        var tokenResult = getTokensTask.Result;

        return (balanceResult, tokenResult);
    }

    private List<TokenModel> ValidateTokens(TokenResult tokenResult)
    {
        var validTokens = new List<TokenModel>();

        foreach (var tokenItem in tokenResult.TokenValues)
        {
            try
            {
                var mint = tokenItem.TokenAccount.AccountData.DataParsed.ParsedInfo.Mint;
                long amount;
                long.TryParse(tokenItem.TokenAccount.AccountData.DataParsed.ParsedInfo.TokenAmount.Amount, out amount);

                if (amount == 0)
                    continue; // Don't track empty tokens
                if (amount != 1 && !_trackedCoins.Contains(mint))
                {
                    continue; // Track only relevant coins
                }

                var tokenAmount = tokenItem.TokenAccount.AccountData.DataParsed.ParsedInfo.TokenAmount;

                var tokenType = amount == 1 ? TokenType.NFT : TokenType.Coin;

                validTokens.Add(new TokenModel(mint, tokenAmount, tokenType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ValidateTokens)} Failed - {JsonSerializer.Serialize(tokenItem)}");
                throw;
            }
        }

        return validTokens;
    }
    
    private async Task GetMetadataAndEnrichTokens(List<TokenModel> validTokens)
    {
        var mintAddresses = validTokens.Where(_ => _.Type == TokenType.NFT).Select(_ => _.Mint).ToList();
        var mintAddressChunks = ListHelpers.GetChunks(mintAddresses, 100);

        foreach (var mintAddressChunk in mintAddressChunks)
        {
            var tokensMetadata = await _nftApiClient.GetTokenMetaData(mintAddressChunk);

            ExtendTokens(validTokens, tokensMetadata);
        }
    }
    
    private void ExtendTokens(List<TokenModel> validTokens, List<MetadataResponse> tokensMetadata)
    {
        foreach (var tokenMetadata in tokensMetadata)
        {
            var validToken = validTokens.FirstOrDefault(_ => _.Mint == tokenMetadata.Mint);
            
            try
            {
                if (validToken == null || tokenMetadata.OffChainData == null)
                {
                    validTokens.RemoveAll(_ => _.Mint == tokenMetadata.Mint);
                    continue;
                }
                
                validToken.Name = tokenMetadata.OffChainData.Name;
                validToken.Symbol = tokenMetadata.OffChainData.Symbol;
                validToken.ImageUrl = tokenMetadata.OffChainData.ImageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ExtendTokens)} Failed. Valid token: {JsonSerializer.Serialize(validToken)}. Token Metadata: {JsonSerializer.Serialize(tokenMetadata)}");
                throw;
            }
        }
    }
}