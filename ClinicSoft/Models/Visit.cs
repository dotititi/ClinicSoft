using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Visit
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateTime VisitTime { get; set; }

    public string ChiefComplaint { get; set; } = null!;

    public int DiagnosisId { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Diagnosis Diagnosis { get; set; } = null!;

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
}
