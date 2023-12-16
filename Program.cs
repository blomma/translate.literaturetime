using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Translate.Models;

var titlesExclusion = new List<string>
{
    "Deuterocanonical Books of the Bible",
    "The Book Of Mormon",
    "The Bible, Douay-Rheims, Old Testament--Part 2",
    "The Bible, Douay-Rheims, Old Testament--Part I",
    "The Bible, Douay-Rheims, New Testament",
    "The Declaration of Independence",
    "The Declaration of Independence",
    "The Antiquities of the Jews",
    "Science and Health With Key to The Scriptures",
    "The Gospels in Four Part Harmony",
    "They Call Me Carpenter",
    "Introduction to Robert Browning",
    "Commentary on the Epistle to the Galatians",
    "The Bible, King James Version",
    "Weymouth New Testament in Modern Speech, Preface and Introductions",
    "An Explanation of Luther's Small Catechism",
    "The Confutatio Pontificia",
    "The Great Doctrines of the Bible",
    "A Treatise on Good Works",
    "The American Woman's Home",
    "Works of Martin Luther"
};

var authorExclusion = new List<string>
{
    "Flavius Josephus",
    "Mary Baker Eddy",
    "J. Clontz",
    "John Bunyan",
    "Joseph Stump",
    "Rev. William Evans",
    "Henry F. Lutz",
    "E. B. Stewart",
    "Henry T. Sell",
    "Benedict of Spinoza",
    "Alexander von Humboldt",
    "Augustus Hopkins Strong",
    "Martin Luther"
};

#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
JsonSerializerOptions jsonSerializerOptions =
    new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
#pragma warning restore CA1869 // Cache and reuse 'JsonSerializerOptions' instances

var files = Directory.EnumerateFiles(
    "/Users/blomma/Downloads/data",
    "*.json",
    SearchOption.AllDirectories
);
List<LiteratureTimeImport> literatureTimeImports = [];
foreach (var file in files)
{
    if (
        file.Contains("fileDirectoryDone")
        || file.Contains("timePhrasesSuperGenericOneOf")
        || file.Contains("timePhrasesGenericOneOf")
        || file.Contains("timePhrasesOneOf")
    )
    {
        continue;
    }

    var content = File.ReadAllText(file);
    var result = JsonSerializer.Deserialize<List<LiteratureTimeImport>>(content);

    if (result != null)
    {
        literatureTimeImports.AddRange(result);
    }
}

using var sha256Hash = SHA256.Create();

Console.WriteLine($"Number of quotes: {literatureTimeImports.Count}");
literatureTimeImports.RemoveAll(
    l =>
        string.IsNullOrWhiteSpace(l.Title)
        || string.IsNullOrWhiteSpace(l.Author)
        || authorExclusion.Contains(l.Author)
        || titlesExclusion.Contains(l.Title)
);

List<LiteratureTime> literatureTimes = new(literatureTimeImports.Count);
Console.WriteLine($"Number of quotes: {literatureTimeImports.Count}");

var lookup = literatureTimeImports.ToLookup(t => t.Time);
List<LiteratureTimeImport> literatureTimeImportsFiltered = [];

foreach (var item in lookup)
{
    var t = item.Take(50);
    literatureTimeImportsFiltered.AddRange(t);
}

foreach (
    var (
        time,
        timeQuote,
        quote,
        title,
        author,
        gutenbergReference,
        matchType
    ) in literatureTimeImportsFiltered
)
{
    var trimmedQuote = quote.Replace("\n", " ");
    var hash = GetHash(
        sha256Hash,
        $"{time}{timeQuote}{trimmedQuote}{title}{author}{gutenbergReference}"
    );

    var qi = trimmedQuote.IndexOf(timeQuote, StringComparison.InvariantCultureIgnoreCase);
    var quoteFirst = qi > 0 ? trimmedQuote[..qi] : "";
    var quoteLast = trimmedQuote[(qi + timeQuote.Length)..];
    var quoteTime = trimmedQuote[qi..(qi + timeQuote.Length)];

    literatureTimes.Add(
        new LiteratureTime(
            time,
            quoteFirst,
            quoteTime,
            quoteLast,
            title,
            author,
            gutenbergReference,
            matchType,
            hash
        )
    );
}

var literatureTimesJson = JsonSerializer.Serialize(literatureTimes, jsonSerializerOptions);
File.WriteAllText("/Users/blomma/Downloads/literatureTimes.json", literatureTimesJson);

static string GetHash(HashAlgorithm hashAlgorithm, string input)
{
    var data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

    var stringBuilder = new StringBuilder();
    foreach (var t in data)
    {
        _ = stringBuilder.Append(t.ToString("x2", CultureInfo.InvariantCulture));
    }

    return stringBuilder.ToString();
}
