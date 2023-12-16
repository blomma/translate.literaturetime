namespace Translate.Models;

public record LiteratureTime(
    string Time,
    string QuoteFirst,
    string QuoteTime,
    string QuoteLast,
    string Title,
    string Author,
    string GutenbergReference,
    int MatchType,
    string Hash
);
