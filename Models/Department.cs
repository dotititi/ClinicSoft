using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? HeadDoctorId { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    public virtual Doctor? HeadDoctor { get; set; }
}
