using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class QuestionnaireResponse
{
    public int Id { get; set; }

    public int AssignedQuestionnaireId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string ResponseText { get; set; } = null!;

    public virtual AssignedQuestionnaire AssignedQuestionnaire { get; set; } = null!;
}
