using System.Text.Json.Serialization;

namespace TokenIndexer.Domain.Responses.RPC;

public record TokenResult
{
    [JsonPropertyName("value")]  
    public List<TokenValue> TokenValues { get; set; }
}

public record TokenValue
{
    [JsonPropertyName("account")]  
    public TokenAccount TokenAccount { get; set; }
}

public record TokenAccount
{
    [JsonPropertyName("data")]  
    public AccountData AccountData { get; set; }
}

public record AccountData
{
    [JsonPropertyName("parsed")]  
    public DataParsed DataParsed { get; set; }
}

public record DataParsed
{
    [JsonPropertyName("info")]  
    public ParsedInfo ParsedInfo { get; set; }
}

public record ParsedInfo
{
    [JsonPropertyName("mint")] 
    public string Mint { get; set; }

    [JsonPropertyName("tokenAmount")] 
    public TokenAmount TokenAmount { get; set; }
}

public record TokenAmount
{
    [JsonPropertyName("amount")] 
    public string Amount { get; set; }

    [JsonPropertyName("decimals")] 
    public int Decimals { get; set; }
    
    [JsonPropertyName("uiAmount")]
    public decimal UiAmount { get; set; }
    
    [JsonPropertyName("uiAmountString")]
    public string UiAmountString { get; set; }
}