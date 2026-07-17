using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class MedicalCard
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string InsuranceNumber { get; set; } = null!;

    public string ChronicConditions { get; set; } = null!;

    public string Allergies { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Patient Patient { get; set; } = null!;
}
