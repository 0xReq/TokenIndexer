using System.Text.Json.Serialization;

namespace TokenIndexer.Domain.Responses.RPC;

public record JsonResponse<T>
{
    [JsonPropertyName("result")]   
    public T Result { get; set; }
}