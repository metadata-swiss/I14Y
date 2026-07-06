using System.Diagnostics.CodeAnalysis;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Bfs.Iop.Core.Lucene.Index.Analyzers;

internal class IopStandardAnalyzer : Analyzer
{
    private readonly LuceneVersion _luceneVersion;

    public IopStandardAnalyzer(LuceneVersion luceneVersion)
    {
        _luceneVersion = luceneVersion;
    }

    [SuppressMessage(
        "Usage",
        "CA2000:Dispose objects before losing scope",
        Justification = "Lucene.Net Analyzer owns/disposes the Tokenizer/TokenStream chain via TokenStreamComponents.")]
    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        var tokenizer = new StandardTokenizer(_luceneVersion, reader);
        var tokenFilter = new StandardFilter(_luceneVersion, tokenizer);
        TokenStream stream = new LowerCaseFilter(_luceneVersion, tokenFilter);
        stream = new ASCIIFoldingFilter(stream);
        return new TokenStreamComponents(tokenizer, stream);
    }
}
