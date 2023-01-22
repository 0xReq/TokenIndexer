using System.Text.Json.Serialization;

namespace TokenIndexer.Domain.Requests.RPC;

public record JsonRequest
{
    [JsonPropertyName("id")] 
    public int Id { get; set; } = 1;
    
    [JsonPropertyName("jsonrpc")] 
    public string Jsonrpc { get; set; } = "2.0";

    [JsonPropertyName("method")] 
    public string Method { get; set; }
    
    [JsonPropertyName("params")] 
    public List<object> Params { get; set; }
}

public record JsonRequestEncoding
{
    [JsonPropertyName("encoding")] 
    public string Encoding { get; set; }
}

public record JsonRequestProgram
{
    [JsonPropertyName("programId")] 
    public string ProgramId { get; set; }
}