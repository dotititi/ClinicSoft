using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class LabOrderItem
{
    public int Id { get; set; }

    public int LabOrderId { get; set; }

    public int TestTypeId { get; set; }

    public virtual LabOrder LabOrder { get; set; } = null!;

    public virtual LabTestType TestType { get; set; } = null!;
}
