using System;
using System.Collections.Generic;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicSoft.Data;

public partial class ClinicSoftContext : DbContext
{
    public ClinicSoftContext()
    {
    }

    public ClinicSoftContext(DbContextOptions<ClinicSoftContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AssignedQuestionnaire> AssignedQuestionnaires { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Diagnosis> Diagnoses { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentSignature> DocumentSignatures { get; set; }

    public virtual DbSet<DosageForm> DosageForms { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<LabOrder> LabOrders { get; set; }

    public virtual DbSet<LabOrderItem> LabOrderItems { get; set; }

    public virtual DbSet<LabResult> LabResults { get; set; }

    public virtual DbSet<LabResultItem> LabResultItems { get; set; }

    public virtual DbSet<LabTestType> LabTestTypes { get; set; }

    public virtual DbSet<MedicalCard> MedicalCards { get; set; }

    public virtual DbSet<MedicalHistory> MedicalHistories { get; set; }

    public virtual DbSet<MedicalSpeciality> MedicalSpecialities { get; set; }

    public virtual DbSet<Medication> Medications { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PrescribedMedication> PrescribedMedications { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<QuestionnaireResponse> QuestionnaireResponses { get; set; }

    public virtual DbSet<QuestionnaireTemplate> QuestionnaireTemplates { get; set; }

    public virtual DbSet<TreatmentPlan> TreatmentPlans { get; set; }

    public virtual DbSet<UnitOfMeasurement> UnitOfMeasurements { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Visit> Visits { get; set; }

    public virtual DbSet<VisitDiagnosis> VisitDiagnoses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=185.185.68.210;Port=5432;Database=ClinicSoftDB;Username=postgres;Password=P@ssw0rd");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("appointment_pkey");

            entity.ToTable("appointment");

            entity.HasIndex(e => new { e.PatientId, e.DoctorId }, "idx_appointment_patient_doctor");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.ScheduledTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("scheduled_time");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'scheduled'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("appointment_doctor_id_fkey");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("appointment_patient_id_fkey");
        });

        modelBuilder.Entity<AssignedQuestionnaire>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("assigned_questionnaire_pkey");

            entity.ToTable("assigned_questionnaire");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("assigned_at");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'assigned'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");

            entity.HasOne(d => d.Patient).WithMany(p => p.AssignedQuestionnaires)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("assigned_questionnaire_patient_id_fkey");

            entity.HasOne(d => d.Template).WithMany(p => p.AssignedQuestionnaires)
                .HasForeignKey(d => d.TemplateId)
                .HasConstraintName("assigned_questionnaire_template_id_fkey");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("department_pkey");

            entity.ToTable("department");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HeadDoctorId).HasColumnName("head_doctor_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.HeadDoctor).WithMany(p => p.Departments)
                .HasForeignKey(d => d.HeadDoctorId)
                .HasConstraintName("department_head_doctor_id_fkey");
        });

        modelBuilder.Entity<Diagnosis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("diagnosis_pkey");

            entity.ToTable("diagnosis");

            entity.HasIndex(e => e.Code, "diagnosis_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("doctor_pkey");

            entity.ToTable("doctor");

            entity.HasIndex(e => e.UserId, "idx_doctor_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .HasColumnName("middle_name");
            entity.Property(e => e.SpecialityId).HasColumnName("speciality_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Department).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("doctor_department_id_fkey");

            entity.HasOne(d => d.Speciality).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.SpecialityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("doctor_speciality_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("doctor_user_id_fkey");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("document_pkey");

            entity.ToTable("document");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(50)
                .HasColumnName("document_type");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Documents)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("document_doctor_id_fkey");

            entity.HasOne(d => d.Patient).WithMany(p => p.Documents)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("document_patient_id_fkey");
        });

        modelBuilder.Entity<DocumentSignature>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("document_signature_pkey");

            entity.ToTable("document_signature");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.SignatureData).HasColumnName("signature_data");
            entity.Property(e => e.SignedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("signed_at");
            entity.Property(e => e.SignedByPatient)
                .HasDefaultValue(false)
                .HasColumnName("signed_by_patient");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentSignatures)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("document_signature_document_id_fkey");
        });

        modelBuilder.Entity<DosageForm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("dosage_form_pkey");

            entity.ToTable("dosage_form");

            entity.HasIndex(e => e.Name, "dosage_form_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("gender_pkey");

            entity.ToTable("gender");

            entity.Property(e => e.Code)
                .HasMaxLength(1)
                .ValueGeneratedNever()
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
        });

        modelBuilder.Entity<LabOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lab_order_pkey");

            entity.ToTable("lab_order");

            entity.HasIndex(e => e.PatientId, "idx_lab_order_patient");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OrderedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ordered_at");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");

            entity.HasOne(d => d.Doctor).WithMany(p => p.LabOrders)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("lab_order_doctor_id_fkey");

            entity.HasOne(d => d.Patient).WithMany(p => p.LabOrders)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("lab_order_patient_id_fkey");

            entity.HasOne(d => d.Visit).WithMany(p => p.LabOrders)
                .HasForeignKey(d => d.VisitId)
                .HasConstraintName("lab_order_visit_id_fkey");
        });

        modelBuilder.Entity<LabOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lab_order_item_pkey");

            entity.ToTable("lab_order_item");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LabOrderId).HasColumnName("lab_order_id");
            entity.Property(e => e.TestTypeId).HasColumnName("test_type_id");

            entity.HasOne(d => d.LabOrder).WithMany(p => p.LabOrderItems)
                .HasForeignKey(d => d.LabOrderId)
                .HasConstraintName("lab_order_item_lab_order_id_fkey");

            entity.HasOne(d => d.TestType).WithMany(p => p.LabOrderItems)
                .HasForeignKey(d => d.TestTypeId)
                .HasConstraintName("lab_order_item_test_type_id_fkey");
        });

        modelBuilder.Entity<LabResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lab_result_pkey");

            entity.ToTable("lab_result");

            entity.HasIndex(e => e.LabOrderId, "lab_result_lab_order_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.LabOrderId).HasColumnName("lab_order_id");
            entity.Property(e => e.PerformedBy)
                .HasMaxLength(100)
                .HasColumnName("performed_by");

            entity.HasOne(d => d.LabOrder).WithOne(p => p.LabResult)
                .HasForeignKey<LabResult>(d => d.LabOrderId)
                .HasConstraintName("lab_result_lab_order_id_fkey");
        });

        modelBuilder.Entity<LabResultItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lab_result_item_pkey");

            entity.ToTable("lab_result_item");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LabResultId).HasColumnName("lab_result_id");
            entity.Property(e => e.NumericValue)
                .HasPrecision(10, 3)
                .HasColumnName("numeric_value");
            entity.Property(e => e.ReferenceRange)
                .HasMaxLength(100)
                .HasColumnName("reference_range");
            entity.Property(e => e.ResultValue).HasColumnName("result_value");
            entity.Property(e => e.TestTypeId).HasColumnName("test_type_id");
            entity.Property(e => e.UnitId).HasColumnName("unit_id");

            entity.HasOne(d => d.LabResult).WithMany(p => p.LabResultItems)
                .HasForeignKey(d => d.LabResultId)
                .HasConstraintName("lab_result_item_lab_result_id_fkey");

            entity.HasOne(d => d.TestType).WithMany(p => p.LabResultItems)
                .HasForeignKey(d => d.TestTypeId)
                .HasConstraintName("lab_result_item_test_type_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.LabResultItems)
                .HasForeignKey(d => d.UnitId)
                .HasConstraintName("lab_result_item_unit_id_fkey");
        });

        modelBuilder.Entity<LabTestType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lab_test_type_pkey");

            entity.ToTable("lab_test_type");

            entity.HasIndex(e => e.Name, "idx_lab_test_type_name");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NormalRange)
                .HasMaxLength(100)
                .HasColumnName("normal_range");
            entity.Property(e => e.UnitId).HasColumnName("unit_id");

            entity.HasOne(d => d.Unit).WithMany(p => p.LabTestTypes)
                .HasForeignKey(d => d.UnitId)
                .HasConstraintName("lab_test_type_unit_id_fkey");
        });

        modelBuilder.Entity<MedicalCard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("medical_card_pkey");

            entity.ToTable("medical_card");

            entity.HasIndex(e => e.PatientId, "medical_card_patient_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Allergies).HasColumnName("allergies");
            entity.Property(e => e.BloodGroup)
                .HasMaxLength(10)
                .HasColumnName("blood_group");
            entity.Property(e => e.ChronicConditions).HasColumnName("chronic_conditions");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.InsuranceNumber)
                .HasMaxLength(30)
                .HasColumnName("insurance_number");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.RhFactor)
                .HasMaxLength(5)
                .HasColumnName("rh_factor");

            entity.HasOne(d => d.Patient).WithOne(p => p.MedicalCard)
                .HasForeignKey<MedicalCard>(d => d.PatientId)
                .HasConstraintName("medical_card_patient_id_fkey");
        });

        modelBuilder.Entity<MedicalHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("medical_history_pkey");

            entity.ToTable("medical_history");

            entity.HasIndex(e => e.PatientId, "idx_medical_history_patient");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.PrimaryDiagnosisId).HasColumnName("primary_diagnosis_id");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Patient).WithMany(p => p.MedicalHistories)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("medical_history_patient_id_fkey");

            entity.HasOne(d => d.PrimaryDiagnosis).WithMany(p => p.MedicalHistories)
                .HasForeignKey(d => d.PrimaryDiagnosisId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("medical_history_primary_diagnosis_id_fkey");
        });

        modelBuilder.Entity<MedicalSpeciality>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("medical_speciality_pkey");

            entity.ToTable("medical_speciality");

            entity.HasIndex(e => e.Name, "medical_speciality_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("medication_pkey");

            entity.ToTable("medication");

            entity.HasIndex(e => e.Name, "idx_medication_name");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DosageFormId).HasColumnName("dosage_form_id");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(255)
                .HasColumnName("manufacturer");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.DosageForm).WithMany(p => p.Medications)
                .HasForeignKey(d => d.DosageFormId)
                .HasConstraintName("medication_dosage_form_id_fkey");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("patient_pkey");

            entity.ToTable("patient");

            entity.HasIndex(e => e.UserId, "idx_patient_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.GenderCode)
                .HasMaxLength(1)
                .HasColumnName("gender_code");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .HasColumnName("middle_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.GenderCodeNavigation).WithMany(p => p.Patients)
                .HasForeignKey(d => d.GenderCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("patient_gender_code_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Patients)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("patient_user_id_fkey");
        });

        modelBuilder.Entity<PrescribedMedication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prescribed_medication_pkey");

            entity.ToTable("prescribed_medication");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Dosage)
                .HasMaxLength(100)
                .HasColumnName("dosage");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.Instructions).HasColumnName("instructions");
            entity.Property(e => e.MedicationId).HasColumnName("medication_id");
            entity.Property(e => e.PrescriptionId).HasColumnName("prescription_id");

            entity.HasOne(d => d.Medication).WithMany(p => p.PrescribedMedications)
                .HasForeignKey(d => d.MedicationId)
                .HasConstraintName("prescribed_medication_medication_id_fkey");

            entity.HasOne(d => d.Prescription).WithMany(p => p.PrescribedMedications)
                .HasForeignKey(d => d.PrescriptionId)
                .HasConstraintName("prescribed_medication_prescription_id_fkey");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prescription_pkey");

            entity.ToTable("prescription");

            entity.HasIndex(e => e.VisitId, "idx_prescription_visit");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.IssuedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("issued_at");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("prescription_doctor_id_fkey");

            entity.HasOne(d => d.Visit).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.VisitId)
                .HasConstraintName("prescription_visit_id_fkey");
        });

        modelBuilder.Entity<QuestionnaireResponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("questionnaire_response_pkey");

            entity.ToTable("questionnaire_response");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignedQuestionnaireId).HasColumnName("assigned_questionnaire_id");
            entity.Property(e => e.QuestionText).HasColumnName("question_text");
            entity.Property(e => e.ResponseText).HasColumnName("response_text");

            entity.HasOne(d => d.AssignedQuestionnaire).WithMany(p => p.QuestionnaireResponses)
                .HasForeignKey(d => d.AssignedQuestionnaireId)
                .HasConstraintName("questionnaire_response_assigned_questionnaire_id_fkey");
        });

        modelBuilder.Entity<QuestionnaireTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("questionnaire_template_pkey");

            entity.ToTable("questionnaire_template");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Doctor).WithMany(p => p.QuestionnaireTemplates)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("questionnaire_template_doctor_id_fkey");
        });

        modelBuilder.Entity<TreatmentPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("treatment_plan_pkey");

            entity.ToTable("treatment_plan");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.MedicalHistoryId).HasColumnName("medical_history_id");
            entity.Property(e => e.PlanDetails).HasColumnName("plan_details");

            entity.HasOne(d => d.Doctor).WithMany(p => p.TreatmentPlans)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("treatment_plan_doctor_id_fkey");

            entity.HasOne(d => d.MedicalHistory).WithMany(p => p.TreatmentPlans)
                .HasForeignKey(d => d.MedicalHistoryId)
                .HasConstraintName("treatment_plan_medical_history_id_fkey");
        });

        modelBuilder.Entity<UnitOfMeasurement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("unit_of_measurement_pkey");

            entity.ToTable("unit_of_measurement");

            entity.HasIndex(e => e.Symbol, "unit_of_measurement_symbol_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Symbol)
                .HasMaxLength(20)
                .HasColumnName("symbol");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.ToTable("user");

            entity.HasIndex(e => e.Login, "user_login_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .HasColumnName("login");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("visit_pkey");

            entity.ToTable("visit");

            entity.HasIndex(e => e.DoctorId, "idx_visit_doctor");

            entity.HasIndex(e => e.PatientId, "idx_visit_patient");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.ChiefComplaint).HasColumnName("chief_complaint");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.VisitTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("visit_time");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Visits)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("visit_appointment_id_fkey");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Visits)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("visit_doctor_id_fkey");

            entity.HasOne(d => d.Patient).WithMany(p => p.Visits)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("visit_patient_id_fkey");
        });

        modelBuilder.Entity<VisitDiagnosis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("visit_diagnosis_pkey");

            entity.ToTable("visit_diagnosis");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DiagnosisId).HasColumnName("diagnosis_id");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(true)
                .HasColumnName("is_primary");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");

            entity.HasOne(d => d.Diagnosis).WithMany(p => p.VisitDiagnoses)
                .HasForeignKey(d => d.DiagnosisId)
                .HasConstraintName("visit_diagnosis_diagnosis_id_fkey");

            entity.HasOne(d => d.Visit).WithMany(p => p.VisitDiagnoses)
                .HasForeignKey(d => d.VisitId)
                .HasConstraintName("visit_diagnosis_visit_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
