using Bfs.Iop.Admin.Models;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Business.UnitTests.Extensions
{
    internal static class MultiLanguageExtensions
    {
        internal static Dictionary<string, string> ToDictionary(this MultiLanguage instance)
        {
            var dictionary = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(instance.De))
            {
                dictionary["de"] = instance.De;
            }

            if (!string.IsNullOrEmpty(instance.Fr))
            {
                dictionary["fr"] = instance.Fr;
            }

            if (!string.IsNullOrEmpty(instance.It))
            {
                dictionary["it"] = instance.It;
            }

            if (!string.IsNullOrEmpty(instance.En))
            {
                dictionary["en"] = instance.En;
            }

            if (!string.IsNullOrEmpty(instance.Rm))
            {
                dictionary["rm"] = instance.Rm;
            }

            return dictionary;
        }
    }
}