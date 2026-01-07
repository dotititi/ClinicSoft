using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class AssignedQuestionnaire
{
    public int Id { get; set; }

    public int TemplateId { get; set; }

    public int PatientId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Status { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<QuestionnaireResponse> QuestionnaireResponses { get; set; } = new List<QuestionnaireResponse>();

    public virtual QuestionnaireTemplate Template { get; set; } = null!;
}
