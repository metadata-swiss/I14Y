using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Samples;

internal static class AgentSamples
{
    public static readonly Guid I14YTestId = new("7fd424b8-0cca-4954-bc80-e889c5c3a510");

    public static IEnumerable<Agent> Generate()
    {

        var test = new Agent
        {
            Id = I14YTestId,
            Identifier = "i14y-test-organisation",
            PrefLabel = new MultiLanguage
            {
                De = "I14Y Test Organisation_de",
                Fr = "I14Y Test Organisation_fr",
                It = "I14Y Test Organisation_it",
                En = "I14Y Test Organisation_en",
                Rm = "I14Y Test Organisation_rm"
            },
            Name = new MultiLanguage
            {
                De = "I14Y Test Organisation_de",
                Fr = "I14Y Test Organisation_fr",
                It = "I14Y Test Organisation_it",
                En = "I14Y Test Organisation_en",
                Rm = "I14Y Test Organisation_rm"
            }
        };


        return [test];
    }
}
