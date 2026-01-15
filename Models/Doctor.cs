using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Doctor
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? DepartmentId { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public int SpecialityId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<QuestionnaireTemplate> QuestionnaireTemplates { get; set; } = new List<QuestionnaireTemplate>();

    public virtual MedicalSpeciality Speciality { get; set; } = null!;

    public virtual ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
