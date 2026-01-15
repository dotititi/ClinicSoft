using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class UnitOfMeasurement
{
    public int Id { get; set; }

    public string Symbol { get; set; } = null!;

    public virtual ICollection<LabResultItem> LabResultItems { get; set; } = new List<LabResultItem>();

    public virtual ICollection<LabTestType> LabTestTypes { get; set; } = new List<LabTestType>();
}
