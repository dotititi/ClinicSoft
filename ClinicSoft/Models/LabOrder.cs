using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class LabOrder
{
    public int Id { get; set; }

    public int VisitId { get; set; }

    public int DoctorId { get; set; }

    public int PatientId { get; set; }

    public DateTime? OrderedAt { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<LabOrderItem> LabOrderItems { get; set; } = new List<LabOrderItem>();

    public virtual LabResult? LabResult { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual Visit Visit { get; set; } = null!;
}
