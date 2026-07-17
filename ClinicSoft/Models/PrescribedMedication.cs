using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class PrescribedMedication
{
    public int Id { get; set; }

    public int TreatmentPlanId { get; set; }

    public int MedicationId { get; set; }

    public string Dosage { get; set; } = null!;

    public int DurationDays { get; set; }

    public string Instructions { get; set; } = null!;

    public virtual Medication Medication { get; set; } = null!;

    public virtual TreatmentPlan TreatmentPlan { get; set; } = null!;
}
