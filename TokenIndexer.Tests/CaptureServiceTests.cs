using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using TokenIndexer.Domain.Clients;
using TokenIndexer.Domain.Entities;
using TokenIndexer.Domain.Entities.Enums;
using TokenIndexer.Domain.Responses.RPC;
using TokenIndexer.Domain.Services.Capture;
using TokenIndexer.Domain.Services.Snapshot;
using TokenIndexer.Domain.Services.Wallet;
using Xunit;

namespace TokenIndexer.Tests;

public class CaptureServiceTests
{
    [Fact(DisplayName = nameof(Should_Calculate_Delta_Between_Snapshots))]
    public void Should_Calculate_Delta_Between_Snapshots()
    {
        var loggerMock = new Mock<ILogger<CaptureService>>().Object;
        var snapshotService = new Mock<ISnapshotService>().Object;
        var walletService = new Mock<IWalletService>().Object;
        var rpcClient = new Mock<IRpcClient>().Object;
        var nftApiClient = new Mock<INftApiClient>().Object;
        
        var captureService = new CaptureService(loggerMock, snapshotService, walletService,
            rpcClient, nftApiClient);

        var previousTokens = new List<TokenModel>
        {
            new TokenModel("mint123", new TokenAmount(), TokenType.NFT),
            new TokenModel("mint123", new TokenAmount(), TokenType.NFT),
            new TokenModel("mint123", new TokenAmount(), TokenType.NFT),
            new TokenModel("mint123", new TokenAmount(), TokenType.NFT),
            new TokenModel("mint123", new TokenAmount(), TokenType.NFT),
            new TokenModel("mint123", new TokenAmount(), TokenType.NFT),
        };
        
        var currentTokens = new List<TokenModel>
        {
            new TokenModel("mint123", new TokenAmount(), TokenType.NFT),
            new TokenModel("mint123", new TokenAmount(), TokenType.NFT),
            new TokenModel("mint123", new TokenAmount(), TokenType.NFT),
            new TokenModel("mint123", new TokenAmount(), TokenType.NFT),
            new TokenModel("mint123", new TokenAmount(), TokenType.NFT),
            new TokenModel("mint456", new TokenAmount(), TokenType.NFT),
            new TokenModel("mint456", new TokenAmount(), TokenType.NFT),
            new TokenModel("mint456", new TokenAmount(), TokenType.NFT),
        };

        // Negative to positive
        var delta = captureService.GetPercentageDelta(previousTokens, currentTokens);

        Assert.Equal((decimal)33.33, delta);
    }
}