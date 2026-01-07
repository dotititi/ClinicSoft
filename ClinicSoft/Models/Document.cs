using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Document
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public string Title { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public byte[]? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<DocumentSignature> DocumentSignatures { get; set; } = new List<DocumentSignature>();

    public virtual Patient Patient { get; set; } = null!;
}
