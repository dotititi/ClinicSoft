using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class MedicalSpeciality
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
