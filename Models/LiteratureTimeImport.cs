namespace Translate.Models;

public record LiteratureTimeImport(
    string Time,
    string TimeQuote,
    string Quote,
    string Title,
    string Author,
    string GutenbergReference,
    int MatchType
);
