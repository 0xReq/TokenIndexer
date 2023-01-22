using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TokenIndexer.Domain.Entities;
using TokenIndexer.Domain.Entities.Enums;
using TokenIndexer.Domain.Helpers;
using TokenIndexer.Domain.Services.Snapshot;
using TokenIndexer.Domain.Services.Wallet;
using TokenIndexer.Web.Entities;
using TokenIndexer.Web.ViewModels.Wallet;

namespace TokenIndexer.Web.Pages;

public partial class Index
{
    [Inject] protected IJSRuntime JSRuntime { get; set; }

    [Inject] protected NavigationManager NavigationManager { get; set; }

    [Inject] protected IWalletService _walletService { get; set; }

    [Inject] protected ISnapshotService _snapshotService { get; set; }

    [Parameter] public string WalletId { get; set; }

    protected string totalWalletCount;
    protected string latestSnapshot;
    protected List<TokenCollection> tokenCollections;
    protected List<WalletDto> allWallets;
    protected WalletDto? walletDto;
    protected SnapshotDto? tokenOverview;
    protected string currentWalletLatestSnapshot;
    protected IEnumerable<SnapshotDto>? tokens;
    protected IEnumerable<LineChartData> solLineChartDatas;
    protected IEnumerable<LineChartData> tokenLineChartDatas;
    protected IEnumerable<ActivityData> dailyActivities;
    protected bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(WalletId))
        {
            var getWalletTask = _walletService.GetByWallet(WalletId);
            var getTokensTask = _snapshotService.GetTokens(WalletId, 1, 10);

            await Task.WhenAll(getWalletTask, getTokensTask).ConfigureAwait(false);

            walletDto = getWalletTask.Result;
            tokens = getTokensTask.Result;
            tokenOverview = getTokensTask.Result.FirstOrDefault();
            currentWalletLatestSnapshot = $"{DateTimeHelpers.GetDateTimeFromTimestamp(tokenOverview.Timestamp).ToString("yyyy-MM-dd h:mm")} UTC";

            tokenCollections = GetGroupedTokens(tokenOverview.Tokens);
            solLineChartDatas = InitSolChart(tokens);
            tokenLineChartDatas = InitTokenChart(tokens);
            dailyActivities = GetDailyActivity(tokens.ToList());
            isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
        {
            (var wallets, var count) = await _walletService.GetPaginated(1, 20);

            var getWalletTask = _walletService.GetTotalCount();
            var getTokenOverviewTask = _snapshotService.GetTokens(1, 1);

            await Task.WhenAll(getWalletTask, getTokenOverviewTask).ConfigureAwait(false);

            totalWalletCount = getWalletTask.Result.ToString();
            var latestItem = getTokenOverviewTask.Result.FirstOrDefault();
            latestSnapshot = $"{DateTimeHelpers.GetDateTimeFromTimestamp(latestItem.Timestamp).ToString("yyyy-MM-dd h:mm")} UTC";
            
            allWallets = wallets.OrderByDescending(_ => _.RecentTokenDelta).ToList();
            await InvokeAsync(StateHasChanged);
        }
    }

    private IEnumerable<ActivityData> GetDailyActivity(List<SnapshotDto> snapshotDtos)
    {
        var current = snapshotDtos[0];
        var previous = snapshotDtos[1];

        IEnumerable<TokenModel> addResults = current.Tokens.Except(previous.Tokens);
        IEnumerable<TokenModel> removeResults = previous.Tokens.Except(current.Tokens);

        var date = DateTimeHelpers.GetDateTimeFromTimestamp(current.Timestamp);
        var dailyActivities = new List<ActivityData>();
        
        foreach (var token in addResults.Where(_ => _.Type == TokenType.NFT))
        {
            if(string.IsNullOrEmpty(token.Name))
                continue;
            
            var dailyActivity = new ActivityData(date, token.Name, ActivityType.Added);
            
            dailyActivities.Add(dailyActivity);
        }
        
        foreach (var token in removeResults.Where(_ => _.Type == TokenType.NFT))
        {
            if(string.IsNullOrEmpty(token.Name))
                continue;
            
            var dailyActivity = new ActivityData(date, token.Name, ActivityType.Removed);
            
            dailyActivities.Add(dailyActivity);
        }

        return dailyActivities;
    }

    private string FormatAsDay(object value)
    {
        if (value != null)
        {
            return Convert.ToDateTime(value).ToString("dd");
        }

        return string.Empty;
    }
    
    private IEnumerable<LineChartData> LoadCollectionChart(string symbol)
    {
        var lineChartDatas = new List<LineChartData>();
        
        foreach (var result in tokens)
        {
            var selectedSymbol = result.Tokens.Where(_ => _.Symbol == symbol);
            var date = DateTimeHelpers.GetDateTimeFromTimestamp(result.Timestamp).ToString("yyyy-MM-dd h:mm");
            var value = selectedSymbol.Count();

            lineChartDatas.Add(new LineChartData(date, value));
        }

        return lineChartDatas.OrderBy(_ => _.Date).ToList();;
    }
    
    private IEnumerable<LineChartData> InitSolChart(IEnumerable<SnapshotDto> results)
    {
        var lineChartDatas = new List<LineChartData>();
        
        foreach (var result in results)
        {
            var date = DateTimeHelpers.GetDateTimeFromTimestamp(result.Timestamp).ToString("yyyy-MM-dd h:mm");
            var value = (double)SolanaHelpers.ConvertToSol(result.Lamports);

            lineChartDatas.Add(new LineChartData(date, value));
        }
        
        return lineChartDatas.OrderBy(_ => _.Date).ToList();
    }
    
    private IEnumerable<LineChartData> InitTokenChart(IEnumerable<SnapshotDto> results)
    {
        var lineChartDatas = new List<LineChartData>();
        
        foreach (var result in results)
        {
            var date = DateTimeHelpers.GetDateTimeFromTimestamp(result.Timestamp).ToString("yyyy-MM-dd h:mm");
            var value = result.Tokens.Count();

            lineChartDatas.Add(new LineChartData(date, value));
        }

        return lineChartDatas.OrderBy(_ => _.Date).ToList();
    }

    private List<TokenCollection> GetGroupedTokens(IEnumerable<TokenModel> tokenModels)
    {
        var groupedBySymbol = tokenModels.GroupBy(_ => _.Symbol);

        var tokenCollections = new List<TokenCollection>();

        foreach (var group in groupedBySymbol)
        {
            var firstItemInGroup = group.FirstOrDefault();

            if (firstItemInGroup == null || string.IsNullOrEmpty(firstItemInGroup.ImageUrl))
                continue;

            var tokenCollection =
                new TokenCollection(firstItemInGroup.Symbol, firstItemInGroup.ImageUrl, group.Count());

            tokenCollections.Add(tokenCollection);
        }

        return tokenCollections.OrderByDescending(_ => _.Count).ToList();
    }
}