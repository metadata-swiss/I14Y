using AwesomeAssertions;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Validation.Extensions;

[TestFixture(TestOf = typeof(StringValidationExtensions))]
internal class StringValidationExtensionsTests
{
    // Tests with IPv6 addresses
    [TestCase("http://[2001:db8::1]", true)]
    [TestCase("https://[2001:db8::1]", true)]
    [TestCase("ftp://[2001:db8::1]", true)]
    [TestCase("ldap://[2001:db8::7]/c=GB?objectClass?one", true)]
    [TestCase("telnet://[2001:db8::1]:23/", true)]
    [TestCase("http://[2001:db8::1]:80/path", true)]
    [TestCase("https://[2001:db8::1]:443/path", true)]
    [TestCase("http://[2001:db8::1]/path", true)]
    [TestCase("http://2001:db8::1", false)]
    [TestCase("http://2001:db8::1", false)]
    [TestCase("https://2001:db8::", false)]
    [TestCase("ftp://[2001-db8::1", false)]
    [TestCase("http://[2001:db8::1::1", false)]
    // Standard-tests
    [TestCase("http://example.com", true)]
    [TestCase("https://example.com", true)]
    [TestCase("http://www.example.com", true)]
    [TestCase("https://www.example.com", true)]
    [TestCase("ftp://example.com", true)]
    [TestCase("http://example", false)]
    [TestCase("ftp://example", false)]
    [TestCase("", false)]
    [TestCase(" ", false)]
    [TestCase(null!, false)]
    [TestCase("ldap://[2001:db8::7]/c=GB?objectClass?one", true)]
    [TestCase("news:comp.infosystems.www.servers.unix", true)]
    [TestCase("tel:+1-816-555-1212", true)]
    [TestCase("telnet://192.0.2.16:80/", true)]
    [TestCase("urn:oasis:names:specification:docbook:dtd:xml:4.1.2", true)]
    public void Test_IsValidUri_ShouldValidateUriCorrectly(string uri, bool expectedResult)
    {
        var result = uri.IsValidUri();
        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(GetNullOrEmptyOrContainsWhiteSpaceTestCases))]
    public void Given_String_When_IsNullOrEmptyOrContainsWhiteSpace_Then_Expected(string str, bool expected)
    {
        var result = str.IsNullOrEmptyOrContainsWhiteSpace();
        result.Should().Be(expected);
    }

    [TestCase("CHE123456789", true)]
    [TestCase("CHE-123.456.789", false)]
    [TestCase("123456789012", false)]
    [TestCase("CHE123908", false)]
    public void Given_uid_When_IsValidUid_Then_Expected(string uid, bool expected)
    {
        var result = uid.IsValidUid();
        result.Should().Be(expected);
    }

    [TestCase("@example.com", false)]
    [TestCase("tony@example.com", true)]
    [TestCase("tony.stark@example.net", true)]
    [TestCase("tony .stark@example.net", false)]
    [TestCase("TONY@EXAMPLE.GOV", true)]
    [TestCase("tony@example.", false)]
    public void Given_string_When_IsValidEmail_Then_Expected(string email, bool expected)
    {
        var result = email.IsValidEmail();
        result.Should().Be(expected);
    }

    [TestCase("to to", false)]
    [TestCase(null!, false)]
    [TestCase("", false)]
    [TestCase("to:to", true)]
    [TestCase("to/to", false)]
    [TestCase("to?to", false)]
    [TestCase("to#to", false)]
    [TestCase("to[to", false)]
    [TestCase("to]to", false)]
    [TestCase("to!to", false)]
    [TestCase("to$to", false)]
    [TestCase("to&to", false)]
    [TestCase("to'to", false)]
    [TestCase("to(to", false)]
    [TestCase("to)to", false)]
    [TestCase("to*to", false)]
    [TestCase("to+to", false)]
    [TestCase("to,to", false)]
    [TestCase("to;to", false)]
    [TestCase("to=to", false)]
    [TestCase("to<to", false)]
    [TestCase("to>to", false)]
    [TestCase("to{to", false)]
    [TestCase("to}to", false)]
    [TestCase("to|to", false)]
    [TestCase("to^to", false)]
    [TestCase("to`to", false)]
    public void Given_string_When_IsValidIdentifier_Then_Expected(string str, bool expected)
    {
        var result = str.IsValidIdentifier();
        result.Should().Be(expected);
    }

    private static IEnumerable<TestCaseData> GetNullOrEmptyOrContainsWhiteSpaceTestCases()
    {
        var testCases = new TestCaseData[]
        {
            new(null, true),
            new("", true),
            new(" ", true),
            new("      ", true),
            new(" toto", true),
            new("to to", true),
            new("toto ", true),
            new("toto", false)
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var testCaseNumber = i + 1;
            yield return testCases[i].SetArgDisplayNames($"TestCase{testCaseNumber}");
        }
    }
}
