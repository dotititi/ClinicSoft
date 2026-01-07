using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class DocumentSignature
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public bool SignedByPatient { get; set; }

    public byte[]? SignatureData { get; set; }

    public DateTime? SignedAt { get; set; }

    public virtual Document Document { get; set; } = null!;
}
