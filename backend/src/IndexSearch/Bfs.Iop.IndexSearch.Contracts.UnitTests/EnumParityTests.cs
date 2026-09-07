using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;
using Security = Bfs.Iop.Infrastructure.Security;

namespace Bfs.Iop.IndexSearch.Contracts.UnitTests;

[TestFixture]
public class EnumParityTests
{
    [Test]
    public void BusinessRole_matches_the_security_role_it_mirrors() =>
        Enum.GetNames<IndexBusinessRole>().OrderBy(x => x, StringComparer.Ordinal)
            .Should().Equal(Enum.GetNames<Security.BusinessRole>().OrderBy(x => x, StringComparer.Ordinal));

    [Test]
    public void BusinessRole_values_match_as_well() =>
        Enum.GetValues<IndexBusinessRole>().Select(x => ((int)x, x.ToString()))
            .Should().Equal(Enum.GetValues<Security.BusinessRole>().Select(x => ((int)x, x.ToString())));
}