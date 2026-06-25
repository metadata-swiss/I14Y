using Bfs.Iop.Admin.Models;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Business.Services;

public interface ILocalizerService
{
    IList<string> OrderedFallbackLanguageCodes { get; }

    LocalizedText? GetLocalizedText(MultilingualText multilingualText, string? cultureCode = null);

    LocalizedText? GetLocalizedText(MultiLanguage multilingualText, string? cultureCode = null);
}