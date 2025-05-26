namespace translate.literaturetime.Models;

internal sealed record LiteratureTime(
    string Time,
    string QuoteFirst,
    string QuoteTime,
    string QuoteLast,
    string Title,
    string Author,
    string GutenbergReference,
    string Hash
);
