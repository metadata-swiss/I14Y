using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using JCG = J2N.Collections.Generic;

namespace Bfs.Iop.Core.Lucene.Index.Parsers;

internal sealed class PartialTermMultiFieldQueryParser : MultiFieldQueryParser
{
    public PartialTermMultiFieldQueryParser(LuceneVersion matchVersion, string[] fields, Analyzer analyzer) : base(matchVersion, fields, analyzer)
    {
    }

    public PartialTermMultiFieldQueryParser(LuceneVersion matchVersion, string[] fields, Analyzer analyzer, IDictionary<string, float> boosts) : base(matchVersion, fields, analyzer, boosts)
    {
    }

    public override bool AllowLeadingWildcard => true;

    public float PartialMatchCorrectionFactor { get; set; } = 1.0f;

    /// <remarks>
    ///     Not all quoted queries result in calling the method below.
    ///     More specifically, when quotes contain multiple words, the non overriden method GetFieldQuery(string field, string queryText, int slop) is called.
    ///     The result of this method is a query matching texts containing the quoted words (respecting the specified order) with a maximum of 'slop' words in between the first and the last quoted word.
    ///     The slop might be specified after the quotes with operator "~" (e.g. "bauer dipl"~3). The default slop can be specified with class parameter PhraseSlop
    /// </remarks>
    protected override Query GetFieldQuery(string field, string queryText, bool quoted)
    {
        var exact = base.GetFieldQuery(field, queryText, quoted);
        if (quoted) return exact;

        var partial = GetWildcardQuery(field, "*" + queryText + "*");
        partial.Boost = partial.Boost * PartialMatchCorrectionFactor;

        return new BooleanQuery()
            {
                { exact, Occur.SHOULD },
                { partial, Occur.SHOULD }
            };
    }

    // The methods below are adapted from base classes:
    //     https://github.com/apache/lucenenet/blob/master/src/Lucene.Net.QueryParser/Classic/MultiFieldQueryParser.cs
    //     https://github.com/apache/lucenenet/blob/master/src/Lucene.Net.QueryParser/Classic/QueryParserBase.cs

    #region MultiFieldQueryParser Adaptations: Ensure that lowercase and ascii folding is applied to query string for relevant (i.e., analyzed) fields only

    /// <remarks>Fuzziness is specified with operator "~" followed, optionally, by a distance n (by default 2); query uses the Damerau-Levenshtein distance to find all terms with a maximum of n changes</remarks>
    protected override Query GetFuzzyQuery(string field, string termStr, float minSimilarity)
        => GetMultiFieldQuery(field, x => NewFuzzyQuery(new Term(x, PreProcessTerm(termStr)), minSimilarity, FuzzyPrefixLength));

    /// <remarks>Prefix query corresponds to term query with a single wildcard ending the term</remarks>
    protected override Query GetPrefixQuery(string field, string termStr)
        => GetMultiFieldQuery(field, termStr, x => NewPrefixQuery(new Term(x, PreProcessTerm(termStr))));

    /// <remarks>Ranges are specified using the following pattern "[" start "TO" end "]" (brackets can be replaced by curly brackets "{", "}" when start (resp. end) should be excluded from the range)</remarks>
    protected override Query GetRangeQuery(string field, string part1, string part2, bool startInclusive, bool endInclusive)
        => GetMultiFieldQuery(field, x => NewRangeQuery(x, PreProcessTerm(part1), PreProcessTerm(part2), startInclusive, endInclusive));

    /// <remarks>Regular expression patterns are embedded in the query string by wrapping them in forward-slashes ("/")</remarks>
    protected override Query GetRegexpQuery(string field, string termStr)
        => GetMultiFieldQuery(field, x => NewRegexpQuery(new Term(x, PreProcessTerm(termStr))));

    /// <remarks>Wildcard query corresponds to a term query with wildcards</remarks>
    protected override Query GetWildcardQuery(string field, string termStr)
        => GetMultiFieldQuery(field, termStr, x => NewWildcardQuery(new Term(x, PreProcessTerm(termStr))));

    private Query GetMultiFieldQuery(string field, string termStr, Func<string, Query> queryBuilder)
    {
        if ("*".Equals(field) && "*".Equals(termStr)) return NewMatchAllDocsQuery();

        return GetMultiFieldQuery(field, queryBuilder);
    }

    private Query GetMultiFieldQuery(string field, Func<string, Query> queryBuilder)
    {
        if (field is null)
        {
            IList<BooleanClause> clauses = new JCG.List<BooleanClause>();
            for (int i = 0; i < m_fields.Length; i++)
            {
                clauses.Add(new BooleanClause(queryBuilder(m_fields[i]), Occur.SHOULD));
            }
            return GetBooleanQuery(clauses, true);
        }
        return queryBuilder(field);
    }

    private string PreProcessTerm(string termStr)
    {
        var asciiFoldedTermStr = new char[termStr.Length * 4];
        ASCIIFoldingFilter.FoldToASCII(termStr.ToCharArray(), 0, asciiFoldedTermStr, 0, termStr.Length);

        var result = new string(asciiFoldedTermStr.TakeWhile(x => x != '\0').ToArray());

        return LowercaseExpandedTerms ? result.ToLower() : result;
    }

    #endregion MultiFieldQueryParser Adaptations: Ensure that lowercase and ascii folding is applied to query string for relevant (i.e., analyzed) fields only
}
