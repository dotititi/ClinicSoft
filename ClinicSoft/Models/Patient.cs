using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Patient
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public DateOnly Birthday { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int GenderCode { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual Gender GenderCodeNavigation { get; set; } = null!;

    public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();

    public virtual MedicalCard? MedicalCard { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
