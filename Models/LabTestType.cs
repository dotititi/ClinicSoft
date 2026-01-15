using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class LabTestType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? NormalRange { get; set; }

    public int? UnitId { get; set; }

    public virtual ICollection<LabOrderItem> LabOrderItems { get; set; } = new List<LabOrderItem>();

    public virtual ICollection<LabResultItem> LabResultItems { get; set; } = new List<LabResultItem>();

    public virtual UnitOfMeasurement? Unit { get; set; }
}
