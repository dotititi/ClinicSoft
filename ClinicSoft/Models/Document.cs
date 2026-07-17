using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Document
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? DocumentTemplateId { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual DocumentTemplate? DocumentTemplate { get; set; }

    public virtual Patient Patient { get; set; } = null!;
}
