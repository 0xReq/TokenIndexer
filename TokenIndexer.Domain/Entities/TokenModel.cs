using TokenIndexer.Domain.Entities.Enums;
using TokenIndexer.Domain.Responses.RPC;

namespace TokenIndexer.Domain.Entities;

public record TokenModel(string Mint, TokenAmount TokenAmount, TokenType? Type)
{
    public string? Name { get; set; } = default!;
    public string? Symbol { get; set; } = default!;
    public string? ImageUrl { get; set; } = default!;
};