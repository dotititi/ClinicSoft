using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class VisitDiagnosis
{
    public int Id { get; set; }

    public int VisitId { get; set; }

    public int DiagnosisId { get; set; }

    public bool IsPrimary { get; set; }

    public string? Notes { get; set; }

    public virtual Diagnosis Diagnosis { get; set; } = null!;

    public virtual Visit Visit { get; set; } = null!;
}
