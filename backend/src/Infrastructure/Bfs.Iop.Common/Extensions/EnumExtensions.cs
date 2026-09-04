using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bfs.Iop.Common.Extensions;

public static class EnumExtensions
{
    public static void EnsureValueIsValid<T>(
        this T value, 
        [CallerArgumentExpression(nameof(value))]string argumentName = "") where T : struct, Enum
    {
        if (!Enum.IsDefined(typeof(T), value))
        {
            throw new InvalidEnumArgumentException(argumentName, Convert.ToInt32(value), typeof(T));
        }
    }
}
