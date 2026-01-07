using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class LabResultItem
{
    public int Id { get; set; }

    public int LabResultId { get; set; }

    public int TestTypeId { get; set; }

    public string ResultValue { get; set; } = null!;

    public decimal? NumericValue { get; set; }

    public int? UnitId { get; set; }

    public string? ReferenceRange { get; set; }

    public virtual LabResult LabResult { get; set; } = null!;

    public virtual LabTestType TestType { get; set; } = null!;

    public virtual UnitOfMeasurement? Unit { get; set; }
}
