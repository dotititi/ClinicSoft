using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Diagnosis
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();

    public virtual ICollection<VisitDiagnosis> VisitDiagnoses { get; set; } = new List<VisitDiagnosis>();
}
