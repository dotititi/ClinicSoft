using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class DosageForm
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Medication> Medications { get; set; } = new List<Medication>();
}
