namespace Bfs.Iop.Test.Abstraction.Helpers;

///<summary> 
/// Auxiliary methods for creating CSS selectors. 
/// This class provides static methods for: 
/// - creating attribute selectors in the format [attribute='value'] 
/// - creating ID selectors for HTML tags in the format tag#id 
/// </summary>
public static class CssSelectorWrapper
{

    public static string Wrap(string attribute, string value)
    {
        return $"[{attribute}='{value}']";
    }

    public static string WrapTag(string tag, string value)
    {
        return $"{tag}#{value}";
    }
}
