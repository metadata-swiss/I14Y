using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class VcardMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Vcard, VCardModel>()
            .Map(d => d.HasAddress, s => s.AdrWork)
            .Map(d => d.Fn, s => s.Fn)
            .Map(d => d.HasEmail, s => s.EmailInternet)
            .Map(d => d.HasTelephone, s => s.TelWorkVoice)
            .Map(d => d.Kind, _ => VCardKind.Organization)
            .Map(d => d.Note, s => s.Note);

        config.NewConfig<VCardModel, Vcard>()
            .Map(d => d.AdrWork, s => s.HasAddress)
            .Map(d => d.EmailInternet, s => s.HasEmail)
            .Map(d => d.Fn, s => s.Fn)
            .Map(d => d.Note, s => s.Note)
            .Ignore(d => d.Org)
            .Map(d => d.TelWorkVoice, s => s.HasTelephone);
    }
}