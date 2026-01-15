using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Visit
{
    public int Id { get; set; }

    public int? AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateTime VisitTime { get; set; }

    public string? ChiefComplaint { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<VisitDiagnosis> VisitDiagnoses { get; set; } = new List<VisitDiagnosis>();
}
