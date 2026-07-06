using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public class DatasetQualityInformationData
{
    public Guid DatasetId { get; set; }

    public IEnumerable<DatasetQualityInformationLink> Documentation { get; set; } = new List<DatasetQualityInformationLink>();

    public DatasetQualityInformation[] QualityInformations { get; set; } = Array.Empty<DatasetQualityInformation>();
}