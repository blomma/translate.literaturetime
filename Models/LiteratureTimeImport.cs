namespace translate.literaturetime.Models;

internal sealed record LiteratureTimeImport(
    string Time,
    string TimeQuote,
    string Quote,
    string Title,
    string Author,
    string GutenbergReference
);
