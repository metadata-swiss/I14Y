using Bfs.Iop.Core.Lucene;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;

internal sealed class RegistrationStatusBoostQuery : CustomScoreQuery
{
    public RegistrationStatusBoostQuery(Query subQuery) : base(subQuery) { }

    protected override CustomScoreProvider GetCustomScoreProvider(AtomicReaderContext context) =>
        new Provider(context);

    private sealed class Provider : CustomScoreProvider
    {
        private readonly NumericDocValues _weight;

        public Provider(AtomicReaderContext context) : base(context)
        {
            _weight = context.AtomicReader.GetNumericDocValues(LuceneFields.Catalog.RegistrationStatusWeight);
        }

        public override float CustomScore(int doc, float subQueryScore, float valSrcScore)
        {
            if (_weight is null) return subQueryScore;
            var w = _weight.Get(doc);   // 85..110
            if (w <= 0) return subQueryScore;
            return subQueryScore * ((float)w / 100f);
        }
    }
}
