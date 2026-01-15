using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Prescription
{
    public int Id { get; set; }

    public int VisitId { get; set; }

    public int DoctorId { get; set; }

    public DateTime? IssuedAt { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<PrescribedMedication> PrescribedMedications { get; set; } = new List<PrescribedMedication>();

    public virtual Visit Visit { get; set; } = null!;
}
