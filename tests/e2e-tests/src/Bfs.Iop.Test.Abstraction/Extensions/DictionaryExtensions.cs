using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bfs.Iop.Test.Abstraction.Extensions;

public static class DictionaryExtensions
{
    public static void AddOrUpdate(this IDictionary<string, string> dic, string key, string value)
    {
        ArgumentNullException.ThrowIfNull(dic, nameof(dic));
        ArgumentNullException.ThrowIfNull(key, nameof(key));

        if (dic.ContainsKey(key))
        {
            dic[key] = value;
        }
        else
        {
            dic.Add(key, value);
        }
    }
}
