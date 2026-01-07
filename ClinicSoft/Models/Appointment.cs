using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Appointment
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateTime ScheduledTime { get; set; }

    public string Status { get; set; } = null!;

    public string? Reason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
