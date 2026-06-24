using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Data.Samples;

internal static class IopPersonSamples
{
    public static readonly Guid MaxMusterId = new("56c59f4f-02a0-46b8-b691-2328b6f02054");

    public static IEnumerable<IopPerson> Generate() => [
        new()
        {
            Email = "max.muster@example.org",
            FamilyName = "Muster",
            GivenName = "Max",
            Id = MaxMusterId,
        }];
}
