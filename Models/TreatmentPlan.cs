using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class TreatmentPlan
{
    public int Id { get; set; }

    public int MedicalHistoryId { get; set; }

    public int DoctorId { get; set; }

    public string PlanDetails { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual MedicalHistory MedicalHistory { get; set; } = null!;
}
