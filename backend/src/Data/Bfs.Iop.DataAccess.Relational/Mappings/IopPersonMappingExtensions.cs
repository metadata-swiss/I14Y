using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class IopPersonMappingExtensions
{
    public static IopPersonModel MapToIopPersonModel(this IopPerson iopPerson)
    {
        ArgumentNullException.ThrowIfNull(iopPerson, nameof(iopPerson));

        return new()
        {
            Email = iopPerson.Email,
            FamilyName = iopPerson.FamilyName,
            GivenName = iopPerson.GivenName,
        };
    }

    public static IopPerson MapToIopPerson(this IopPersonModel model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        return new()
        {
            Email = model.Email,
            FamilyName = model.FamilyName,
            GivenName = model.GivenName
        };
    }
}
