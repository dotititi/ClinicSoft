using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class PrescribedMedication
{
    public int Id { get; set; }

    public int PrescriptionId { get; set; }

    public int MedicationId { get; set; }

    public string Dosage { get; set; } = null!;

    public int? DurationDays { get; set; }

    public string? Instructions { get; set; }

    public virtual Medication Medication { get; set; } = null!;

    public virtual Prescription Prescription { get; set; } = null!;
}
