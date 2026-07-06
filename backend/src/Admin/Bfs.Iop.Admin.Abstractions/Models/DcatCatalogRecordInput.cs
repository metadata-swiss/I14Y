using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public class DcatCatalogRecordInput
{
    public Guid CatalogId { get; set; }

    public MultiLanguage CatalogTitle { get; set; } = new();

    public Guid Id { get; set; }

    public DcatCatalogResource PrimaryTopic { get; set; } = new();

    public List<DcatVocabularyEntry>? Themes { get; set; }
}