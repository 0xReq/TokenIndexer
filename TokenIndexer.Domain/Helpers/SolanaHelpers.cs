namespace TokenIndexer.Domain.Helpers;

public static class SolanaHelpers
{
    public static decimal? ConvertToSol(ulong? lamports)
    {
        if (lamports == null)
            return null;
        
        return decimal.Round((decimal)lamports / 1000000000m, 0);
    }
}