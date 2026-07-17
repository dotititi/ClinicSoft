using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class DocumentTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int DocumentTypeId { get; set; }

    public string Description { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual DocumentType DocumentType { get; set; } = null!;

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
