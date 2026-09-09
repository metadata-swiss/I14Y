namespace Bfs.Iop.Admin.Models;

public interface ICodeListEntryInputModel
{
    public MultiLanguage? Description { get; }

    public MultiLanguage Name { get; }

    public string? ParentCode { get; }
}
