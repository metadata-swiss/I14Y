using Bfs.Iop.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Services;

public class LocalizerService : ILocalizerService
{
    public LocalizerService(LocalizerConfiguration localizerOptions)
    {
        ArgumentNullException.ThrowIfNull(localizerOptions);

        OrderedFallbackLanguageCodes = localizerOptions.OrderedFallbackLanguageCodes.ToList();
    }

    public IList<string> OrderedFallbackLanguageCodes { get; }

    public LocalizedText? GetLocalizedText(MultilingualText multilingualText, string? cultureCode = null)
    {
        return GetLocalizedText(multilingualText as Dictionary<string, string>, cultureCode);
    }

    public LocalizedText? GetLocalizedText(MultiLanguage multilingualText, string? cultureCode = null)
    {
        var multiText = multilingualText.ToDictionary();
        return GetLocalizedText(multiText, cultureCode);
    }

    private static LocalizedText? GetEntryInCulture(IDictionary<string, string> multiText, string cultureCode)
    {
        var text = multiText.SingleOrDefault(t => t.Key.Trim().ToLower() == cultureCode.Trim().ToLower());

        return string.IsNullOrEmpty(text.Key) ? null : new LocalizedText { CultureCode = text.Key, Text = text.Value };
    }

    private LocalizedText? GetLocalizedText(IDictionary<string, string> multiText, string? cultureCode = null)
    {
        LocalizedText? text = cultureCode != null ? GetEntryInCulture(multiText, cultureCode) : null;

        var enumerator = OrderedFallbackLanguageCodes.GetEnumerator();
        while (enumerator.MoveNext() && text == null)
        {
            text = GetEntryInCulture(multiText, enumerator.Current);
        }

        return text;
    }
}