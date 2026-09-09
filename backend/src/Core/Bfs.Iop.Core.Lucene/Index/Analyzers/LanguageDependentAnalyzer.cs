using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.De;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Fr;
using Lucene.Net.Analysis.It;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using System.Diagnostics.CodeAnalysis;

namespace Bfs.Iop.Core.Lucene.Index.Analyzers;

internal sealed class LanguageDependentAnalyzer : Analyzer
{
    private const int MinGram = 2;
    private const int MaxGram = 3;
    private readonly string _language;
    private readonly bool _isNgramField;
    private readonly LuceneVersion _matchVersion;

    public LanguageDependentAnalyzer(LuceneVersion matchVersion, string language, bool isNgramField = false)
    {
        _language = language;
        _isNgramField = isNgramField;
        _matchVersion = matchVersion;
    }

    [SuppressMessage(
    "Usage",
    "CA2000:Dispose objects before losing scope",
    Justification = "Lucene.Net TokenStreamComponents takes ownership and disposes all Tokenizers and TokenStreams.")]
    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        Tokenizer tokenizer = new StandardTokenizer(_matchVersion, reader);

        TokenFilter tokenFilter = new StandardFilter(_matchVersion, tokenizer);
        tokenFilter = BuildElisionFilter(tokenFilter);
        tokenFilter = new LowerCaseFilter(_matchVersion, tokenFilter);
        tokenFilter = BuildStopFilter(tokenFilter);
        tokenFilter = BuildNormalizationFilter(tokenFilter);
        tokenFilter = new ASCIIFoldingFilter(tokenFilter);

        if (_isNgramField) //this enables partial and "fuzzy" search based on N-Gram tokenization
        {
            tokenFilter = new NGramTokenFilter(_matchVersion, tokenFilter, MinGram, MaxGram);
        }

        return new TokenStreamComponents(tokenizer, tokenFilter);
    }

    private TokenFilter BuildElisionFilter(TokenFilter filter)
    {
        if (_language.Equals("en", StringComparison.InvariantCultureIgnoreCase))
        {
            return new EnglishPossessiveFilter(_matchVersion, filter);
        }

        if (_language.Equals("fr", StringComparison.InvariantCultureIgnoreCase))
        {
            return new ElisionFilter(filter, FrenchAnalyzer.DEFAULT_ARTICLES);
        }

        return filter;
    }

    private TokenFilter BuildNormalizationFilter(TokenFilter filter)
    {
        if (_language.Equals("de", StringComparison.InvariantCultureIgnoreCase))
        {
            return new GermanNormalizationFilter(filter);
        }

        return filter;
    }

    private TokenFilter BuildStopFilter(TokenFilter filter)
    {
        if (_language.Equals("de", StringComparison.InvariantCultureIgnoreCase))
        {
            return new StopFilter(_matchVersion, filter, GermanAnalyzer.DefaultStopSet);
        }

        if (_language.Equals("en", StringComparison.InvariantCultureIgnoreCase))
        {
            return new StopFilter(_matchVersion, filter, EnglishAnalyzer.DefaultStopSet);
        }

        if (_language.Equals("fr", StringComparison.InvariantCultureIgnoreCase))
        {
            return new StopFilter(_matchVersion, filter, FrenchAnalyzer.DefaultStopSet);
        }

        if (_language.Equals("it", StringComparison.InvariantCultureIgnoreCase))
        {
            return new StopFilter(_matchVersion, filter, ItalianAnalyzer.DefaultStopSet);
        }

        return filter;
    }
}