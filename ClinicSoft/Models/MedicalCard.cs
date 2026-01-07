using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class MedicalCard
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string? InsuranceNumber { get; set; }

    public string? BloodGroup { get; set; }

    public string? RhFactor { get; set; }

    public string? ChronicConditions { get; set; }

    public string? Allergies { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Patient Patient { get; set; } = null!;
}
