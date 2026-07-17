using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class LabResult
{
    public int Id { get; set; }

    public int LabOrderId { get; set; }

    public DateTime CompletedAt { get; set; }

    public int PerformedBy { get; set; }

    public virtual LabOrder LabOrder { get; set; } = null!;

    public virtual ICollection<LabResultItem> LabResultItems { get; set; } = new List<LabResultItem>();

    public virtual Doctor PerformedByNavigation { get; set; } = null!;
}
