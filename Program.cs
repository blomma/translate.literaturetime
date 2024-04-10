using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Translate.Models;

JsonSerializerOptions jsonSerializerOptions =
    new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

var files = Directory.EnumerateFiles(
    "../quotes.literaturetime",
    "*.json",
    SearchOption.AllDirectories
);

List<LiteratureTimeImport> literatureTimeImports = [];
foreach (var file in files)
{
    if (
        file.Contains("fileDirectoryDone", StringComparison.InvariantCultureIgnoreCase)
        || file.Contains(
            "timePhrasesSuperGenericOneOf",
            StringComparison.InvariantCultureIgnoreCase
        )
        || file.Contains("timePhrasesGenericOneOf", StringComparison.InvariantCultureIgnoreCase)
        || file.Contains("timePhrasesOneOf", StringComparison.InvariantCultureIgnoreCase)
        || file.Contains("settings", StringComparison.InvariantCultureIgnoreCase)
    )
    {
        continue;
    }

    var content = File.ReadAllText(file);
    try
    {
        var result = JsonSerializer.Deserialize<List<LiteratureTimeImport>>(content);
        if (result != null)
        {
            literatureTimeImports.AddRange(result);
        }
    }
    catch (Exception)
    {
        Console.WriteLine($"file:{file}");
        throw;
    }
}

using var sha256Hash = SHA256.Create();

Console.WriteLine($"Number of quotes: {literatureTimeImports.Count}");

var lookup = literatureTimeImports.ToLookup(t => t.Time);
List<LiteratureTimeImport> literatureTimeImportsFiltered = [];

foreach (var item in lookup)
{
    var quotes = new ReadOnlySpan<LiteratureTimeImport>([.. item]);
#pragma warning disable CA5394
    quotes = quotes.Length >= 20 ? Random.Shared.GetItems(quotes, 20) : quotes;
#pragma warning restore CA5394
    literatureTimeImportsFiltered.AddRange(quotes);
}

Console.WriteLine($"Number of quotes: {literatureTimeImportsFiltered.Count}");

List<LiteratureTime> literatureTimes = [];

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
    var trimmedQuote = quote.Replace("\n", " ", StringComparison.InvariantCultureIgnoreCase);
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

var startOfDay = DateTime.Now.Date;
var endOfDay = startOfDay.Date.AddDays(1).AddTicks(-1);

List<string> literatureTimesComplete = [];
while (startOfDay < endOfDay)
{
    literatureTimesComplete.Add(startOfDay.ToString("HH:mm", CultureInfo.InvariantCulture));
    startOfDay = startOfDay.AddMinutes(1);
}

var literatureTimesMissing = literatureTimesComplete
    .Except(literatureTimes.Select(s => s.Time).Distinct())
    .ToList();

var literatureTimesMissingJson = JsonSerializer.Serialize(
    literatureTimesMissing,
    jsonSerializerOptions
);
File.WriteAllText(
    "../translated.quotes.literaturetime/literatureTimesMissing.json",
    literatureTimesMissingJson
);

var literatureTimesJson = JsonSerializer.Serialize(literatureTimes, jsonSerializerOptions);
File.WriteAllText("../translated.quotes.literaturetime/literatureTimes.json", literatureTimesJson);

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
