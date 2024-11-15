namespace translate.literaturetime.Models;

#pragma warning disable CA1812
internal sealed record LiteratureTimeImport(
    string Time,
    string TimeQuote,
    string Quote,
    string Title,
    string Author,
    string GutenbergReference,
    int MatchType
);
#pragma warning restore CA1812
