using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class QuestionnaireTemplate
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AssignedQuestionnaire> AssignedQuestionnaires { get; set; } = new List<AssignedQuestionnaire>();

    public virtual Doctor Doctor { get; set; } = null!;
}
