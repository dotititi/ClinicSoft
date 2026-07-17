using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class UnitsOfMeasurement
{
    public int Id { get; set; }

    public string Symbol { get; set; } = null!;

    public virtual ICollection<LabTestType> LabTestTypes { get; set; } = new List<LabTestType>();
}
