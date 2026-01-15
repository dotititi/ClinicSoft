using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class MedicalHistory
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int? PrimaryDiagnosisId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual Diagnosis? PrimaryDiagnosis { get; set; }

    public virtual ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
}
