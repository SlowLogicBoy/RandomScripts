#! "netcoreapp2.0"
#r "nuget: AngleSharp, *"

using AngleSharp;
using AngleSharp.Dom;

class TranslationModel {
    //『全速力協奏曲』 / 初音ミク
    public string Original { get; }
    // "Full Speed Concerto" / Miku Hatsune
    public string Result { get; }
    //“Zensokuryoku kyōsōkyoku”/ hatsunemiku
    public string Translit { get; }
    public TranslationModel(string original, string result, string translit) {
        Translit = translit;
        Original = original;
        Result = result;
    }
}

class TranslatorService {
    private const string url = "http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}";
    IBrowsingContext _browser;
    public TranslatorService() {
        _browser = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
    }
    // LangPair = "SL|TL" ( Source Lang | Target Lang - Ex.: "en|pt"
    private Task<IDocument> GetResponse(string text, string languagePair) => _browser.OpenAsync(string.Format(url, Uri.EscapeUriString(text), languagePair));
    private string GetLanguagePair(string input) {
        if (string.IsNullOrWhiteSpace(input)) return null;
        if (input.ContainsJapaneseCharacters()) return "ja|en";
        if (input.ContainsKoreanCharacters()) return "ko|en";
        return null;
    }
    public async Task<TranslationModel> Translate(string input) {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var languagePair = GetLanguagePair(input);

        if (string.IsNullOrWhiteSpace(languagePair)) return null;

        var doc = await GetResponse(input, languagePair);
        var translated = doc.All.First(m => m.Id == "result_box").TextContent;
        var translit = doc.All.First(m => m.Id == "src-translit").TextContent;
        return new TranslationModel(input, translated, translit);
    }
}
internal static bool IsHiragana(this char c) => (c >= 0x3040 && c <= 0x309F) || IsProlongedChar(c);
internal static bool IsKatakana(this char c) => (c >= 0x30A0 && c <= 0x30FF) || IsProlongedChar(c);
internal static bool IsProlongedChar(this char c) => c == 0x30FC;
internal static bool IsKanji(this char c) => c >= 0x4E00 && c <= 0x9FBF;
internal static bool IsJapanese(this char c) => IsHiragana(c) || IsKatakana(c) || IsKanji(c);
internal static bool IsHangulJamo(this char c) => (c >= 0x1100 && c <= 0x11FF) || (c >= 0x3130 && c <= 0x318F) || (c >= 0xA960 && c <= 0xA97F);
internal static bool IsHangulSyllable(this char c) => c >= 0xAC00 && c <= 0xD7FF;
internal static bool ContainsKoreanCharacters(this string s) => s.Any(c => c.IsHangulJamo() || c.IsHangulSyllable());
internal static bool ContainsJapaneseCharacters(this string s) => s.Any(c => c.IsJapanese());
