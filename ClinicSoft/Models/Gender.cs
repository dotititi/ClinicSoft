using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Gender
{
    public char Code { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
