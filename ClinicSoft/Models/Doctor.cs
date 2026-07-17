using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Doctor
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int DepartmentId { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public int SpecialityId { get; set; }

    public int StatusId { get; set; }

    public int OfficeId { get; set; }

    public string Email { get; set; } = null!;

    public DateOnly Birthday { get; set; }

    public string Phone { get; set; } = null!;

    public int GenderCode { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual Gender GenderCodeNavigation { get; set; } = null!;

    public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();

    public virtual ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();

    public virtual Office Office { get; set; } = null!;

    public virtual MedicalSpeciality Speciality { get; set; } = null!;

    public virtual DoctorStatus Status { get; set; } = null!;

    public virtual ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
