using AutoMapper;
using Bfs.Iop.Admin.Business.Services;
using Bfs.Iop.Admin.Models;
using System;

namespace Bfs.Iop.Admin.Business.Mappings;

public class MultilingualTextToLocalizedTextConverter : ITypeConverter<MultilingualText, LocalizedText>
{
    private readonly ILocalizerService _localizerService;

    public MultilingualTextToLocalizedTextConverter(ILocalizerService localizerService)
    {
        _localizerService = localizerService ?? throw new ArgumentNullException(nameof(localizerService));
    }

    public virtual LocalizedText Convert(MultilingualText source, LocalizedText destination, ResolutionContext context)
    {
        if (source != null)
        {
            if (!context.Items.ContainsKey("language"))
            {
                throw new ArgumentException("The converter needs the context item 'language' in order to be able to map a MultilingualText to a LocalizedText.");
            }
            var language = (string)context.Items["language"];

            return _localizerService.GetLocalizedText(source, language);
        }
        return destination;
    }
}