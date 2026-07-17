using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Office
{
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
