using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    public virtual Registrator? Registrator { get; set; }

    public virtual Role Role { get; set; } = null!;
}
