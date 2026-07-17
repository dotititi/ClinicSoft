using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Medication
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int DosageFormId { get; set; }

    public virtual DosageForm DosageForm { get; set; } = null!;

    public virtual ICollection<PrescribedMedication> PrescribedMedications { get; set; } = new List<PrescribedMedication>();
}
