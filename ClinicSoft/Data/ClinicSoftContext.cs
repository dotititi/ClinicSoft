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

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Diagnosis> Diagnoses { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<DoctorSchedule> DoctorSchedules { get; set; }

    public virtual DbSet<DoctorStatus> DoctorStatuses { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentTemplate> DocumentTemplates { get; set; }

    public virtual DbSet<DocumentType> DocumentTypes { get; set; }

    public virtual DbSet<DosageForm> DosageForms { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<LabOrder> LabOrders { get; set; }

    public virtual DbSet<LabOrderItem> LabOrderItems { get; set; }

    public virtual DbSet<LabResult> LabResults { get; set; }

    public virtual DbSet<LabResultItem> LabResultItems { get; set; }

    public virtual DbSet<LabTestType> LabTestTypes { get; set; }

    public virtual DbSet<MedicalCard> MedicalCards { get; set; }

    public virtual DbSet<MedicalSpeciality> MedicalSpecialities { get; set; }

    public virtual DbSet<Medication> Medications { get; set; }

    public virtual DbSet<Office> Offices { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PrescribedMedication> PrescribedMedications { get; set; }

    public virtual DbSet<Registrator> Registrators { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TreatmentPlan> TreatmentPlans { get; set; }

    public virtual DbSet<UnitsOfMeasurement> UnitsOfMeasurements { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Visit> Visits { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=185.185.68.210;Port=5432;Database=ClinicSoftDB;Username=postgres;Password=P@ssw0rd");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admins_pkey");

            entity.ToTable("admins");

            entity.HasIndex(e => e.GenderCode, "IX_admins_gender_code");

            entity.HasIndex(e => e.Email, "admins_email_key").IsUnique();

            entity.HasIndex(e => e.Phone, "admins_phone_key").IsUnique();

            entity.HasIndex(e => e.UserId, "admins_user_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.GenderCode).HasColumnName("gender_code");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(100)
                .HasColumnName("middle_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.GenderCodeNavigation).WithMany(p => p.Admins)
                .HasForeignKey(d => d.GenderCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_admin_gender");

            entity.HasOne(d => d.User).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.UserId)
                .HasConstraintName("admins_user_id_fkey");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("appointments_pkey");

            entity.ToTable("appointments");

            entity.HasIndex(e => new { e.DoctorId, e.ScheduledTime }, "appointments_doctor_time_key").IsUnique();

            entity.HasIndex(e => new { e.PatientId, e.DoctorId }, "idx_appointments_patient_doctor");

            entity.HasIndex(e => e.ScheduledTime, "idx_appointments_scheduled_time");

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
                .HasConstraintName("appointments_doctor_id_fkey");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("appointments_patient_id_fkey");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("departments_pkey");

            entity.ToTable("departments");

            entity.HasIndex(e => e.Name, "departments_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Diagnosis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("diagnoses_pkey");

            entity.ToTable("diagnoses");

            entity.HasIndex(e => e.Name, "diagnoses_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("doctors_pkey");

            entity.ToTable("doctors");

            entity.HasIndex(e => e.DepartmentId, "IX_doctors_department_id");

            entity.HasIndex(e => e.GenderCode, "IX_doctors_gender_code");

            entity.HasIndex(e => e.OfficeId, "IX_doctors_office_id");

            entity.HasIndex(e => e.SpecialityId, "IX_doctors_speciality_id");

            entity.HasIndex(e => e.StatusId, "IX_doctors_status_id");

            entity.HasIndex(e => e.Phone, "doctors_phone_key").IsUnique();

            entity.HasIndex(e => e.Email, "idx_doctors_email")
                .IsUnique()
                .HasFilter("((email IS NOT NULL) AND ((email)::text <> ''::text))");

            entity.HasIndex(e => e.UserId, "idx_doctors_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.GenderCode).HasColumnName("gender_code");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(100)
                .HasColumnName("middle_name");
            entity.Property(e => e.OfficeId).HasColumnName("office_id");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.SpecialityId).HasColumnName("speciality_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Department).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("doctors_department_id_fkey");

            entity.HasOne(d => d.GenderCodeNavigation).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.GenderCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("doctors_gender_code_fkey");

            entity.HasOne(d => d.Office).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.OfficeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("doctors_office_id_fkey");

            entity.HasOne(d => d.Speciality).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.SpecialityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("doctors_speciality_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("doctors_status_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("doctors_user_id_fkey");
        });

        modelBuilder.Entity<DoctorSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("doctor_schedules_pkey");

            entity.ToTable("doctor_schedules");

            entity.HasIndex(e => new { e.DoctorId, e.DayOfWeek }, "idx_doctor_schedules_doctor_day").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsWorking)
                .HasDefaultValue(true)
                .HasColumnName("is_working");
            entity.Property(e => e.StartTime).HasColumnName("start_time");

            entity.HasOne(d => d.Doctor).WithMany(p => p.DoctorSchedules)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("doctor_schedules_doctor_id_fkey");
        });

        modelBuilder.Entity<DoctorStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("doctor_statuses_pkey");

            entity.ToTable("doctor_statuses");

            entity.HasIndex(e => e.Name, "doctor_statuses_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("documents_pkey");

            entity.ToTable("documents");

            entity.HasIndex(e => e.DocumentTemplateId, "IX_documents_document_template_id");

            entity.HasIndex(e => e.CreatedAt, "idx_documents_created_at");

            entity.HasIndex(e => e.DoctorId, "idx_documents_doctor_id");

            entity.HasIndex(e => e.PatientId, "idx_documents_patient_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.DocumentTemplateId).HasColumnName("document_template_id");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Documents)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("documents_doctor_id_fkey");

            entity.HasOne(d => d.DocumentTemplate).WithMany(p => p.Documents)
                .HasForeignKey(d => d.DocumentTemplateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("documents_document_template_id_fkey");

            entity.HasOne(d => d.Patient).WithMany(p => p.Documents)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("documents_patient_id_fkey");
        });

        modelBuilder.Entity<DocumentTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("document_templates_pkey");

            entity.ToTable("document_templates");

            entity.HasIndex(e => e.DocumentTypeId, "IX_document_templates_document_type_id");

            entity.HasIndex(e => e.Name, "document_templates_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DocumentTypeId).HasColumnName("document_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.DocumentTemplates)
                .HasForeignKey(d => d.DocumentTypeId)
                .HasConstraintName("document_templates_document_type_id_fkey");
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("document_types_pkey");

            entity.ToTable("document_types");

            entity.HasIndex(e => e.Name, "document_types_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<DosageForm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("dosage_forms_pkey");

            entity.ToTable("dosage_forms");

            entity.HasIndex(e => e.Name, "dosage_forms_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("genders_pkey");

            entity.ToTable("genders");

            entity.HasIndex(e => e.Name, "genders_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<LabOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lab_orders_pkey");

            entity.ToTable("lab_orders");

            entity.HasIndex(e => e.DoctorId, "idx_lab_orders_doctor_id");

            entity.HasIndex(e => e.PatientId, "idx_lab_orders_patient_id");

            entity.HasIndex(e => e.VisitId, "idx_lab_orders_visit_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
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
                .HasConstraintName("lab_orders_doctor_id_fkey");

            entity.HasOne(d => d.Patient).WithMany(p => p.LabOrders)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("lab_orders_patient_id_fkey");

            entity.HasOne(d => d.Visit).WithMany(p => p.LabOrders)
                .HasForeignKey(d => d.VisitId)
                .HasConstraintName("lab_orders_visit_id_fkey");
        });

        modelBuilder.Entity<LabOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lab_order_items_pkey");

            entity.ToTable("lab_order_items");

            entity.HasIndex(e => e.TestTypeId, "IX_lab_order_items_test_type_id");

            entity.HasIndex(e => e.LabOrderId, "idx_lab_order_items_lab_order_id");

            entity.HasIndex(e => new { e.LabOrderId, e.TestTypeId }, "lab_order_items_order_test_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LabOrderId).HasColumnName("lab_order_id");
            entity.Property(e => e.TestTypeId).HasColumnName("test_type_id");

            entity.HasOne(d => d.LabOrder).WithMany(p => p.LabOrderItems)
                .HasForeignKey(d => d.LabOrderId)
                .HasConstraintName("lab_order_items_lab_order_id_fkey");

            entity.HasOne(d => d.TestType).WithMany(p => p.LabOrderItems)
                .HasForeignKey(d => d.TestTypeId)
                .HasConstraintName("lab_order_items_test_type_id_fkey");
        });

        modelBuilder.Entity<LabResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lab_results_pkey");

            entity.ToTable("lab_results");

            entity.HasIndex(e => e.PerformedBy, "idx_lab_results_performed_by");

            entity.HasIndex(e => e.LabOrderId, "lab_results_lab_order_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.LabOrderId).HasColumnName("lab_order_id");
            entity.Property(e => e.PerformedBy).HasColumnName("performed_by");

            entity.HasOne(d => d.LabOrder).WithOne(p => p.LabResult)
                .HasForeignKey<LabResult>(d => d.LabOrderId)
                .HasConstraintName("lab_results_lab_order_id_fkey");

            entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.LabResults)
                .HasForeignKey(d => d.PerformedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("lab_results_performed_by_fkey");
        });

        modelBuilder.Entity<LabResultItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lab_result_items_pkey");

            entity.ToTable("lab_result_items");

            entity.HasIndex(e => e.TestTypeId, "IX_lab_result_items_test_type_id");

            entity.HasIndex(e => e.LabResultId, "idx_lab_result_items_lab_result_id");

            entity.HasIndex(e => new { e.LabResultId, e.TestTypeId }, "lab_result_items_result_test_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LabResultId).HasColumnName("lab_result_id");
            entity.Property(e => e.ResultValue).HasColumnName("result_value");
            entity.Property(e => e.TestTypeId).HasColumnName("test_type_id");

            entity.HasOne(d => d.LabResult).WithMany(p => p.LabResultItems)
                .HasForeignKey(d => d.LabResultId)
                .HasConstraintName("lab_result_items_lab_result_id_fkey");

            entity.HasOne(d => d.TestType).WithMany(p => p.LabResultItems)
                .HasForeignKey(d => d.TestTypeId)
                .HasConstraintName("lab_result_items_test_type_id_fkey");
        });

        modelBuilder.Entity<LabTestType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lab_test_types_pkey");

            entity.ToTable("lab_test_types");

            entity.HasIndex(e => e.UnitId, "IX_lab_test_types_unit_id");

            entity.HasIndex(e => e.Name, "idx_lab_test_types_name");

            entity.HasIndex(e => e.Name, "lab_test_types_name_key").IsUnique();

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
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("lab_test_types_unit_id_fkey");
        });

        modelBuilder.Entity<MedicalCard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("medical_cards_pkey");

            entity.ToTable("medical_cards");

            entity.HasIndex(e => e.InsuranceNumber, "idx_medical_cards_insurance_number");

            entity.HasIndex(e => e.InsuranceNumber, "medical_cards_insurance_number_key").IsUnique();

            entity.HasIndex(e => e.PatientId, "medical_cards_patient_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Allergies).HasColumnName("allergies");
            entity.Property(e => e.ChronicConditions).HasColumnName("chronic_conditions");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.InsuranceNumber)
                .HasMaxLength(30)
                .HasColumnName("insurance_number");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");

            entity.HasOne(d => d.Patient).WithOne(p => p.MedicalCard)
                .HasForeignKey<MedicalCard>(d => d.PatientId)
                .HasConstraintName("medical_cards_patient_id_fkey");
        });

        modelBuilder.Entity<MedicalSpeciality>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("medical_specialities_pkey");

            entity.ToTable("medical_specialities");

            entity.HasIndex(e => e.Name, "medical_specialities_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("medications_pkey");

            entity.ToTable("medications");

            entity.HasIndex(e => e.DosageFormId, "IX_medications_dosage_form_id");

            entity.HasIndex(e => e.Name, "idx_medications_name");

            entity.HasIndex(e => new { e.Name, e.DosageFormId }, "medications_name_dosage_form_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DosageFormId).HasColumnName("dosage_form_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.DosageForm).WithMany(p => p.Medications)
                .HasForeignKey(d => d.DosageFormId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("medications_dosage_form_id_fkey");
        });

        modelBuilder.Entity<Office>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("offices_pkey");

            entity.ToTable("offices");

            entity.HasIndex(e => e.Number, "offices_number_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Number)
                .HasMaxLength(20)
                .HasColumnName("number");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("patients_pkey");

            entity.ToTable("patients");

            entity.HasIndex(e => e.GenderCode, "IX_patients_gender_code");

            entity.HasIndex(e => e.UserId, "idx_patients_user_id");

            entity.HasIndex(e => e.Email, "patients_email_key").IsUnique();

            entity.HasIndex(e => e.Phone, "patients_phone_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.GenderCode).HasColumnName("gender_code");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(100)
                .HasColumnName("middle_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.GenderCodeNavigation).WithMany(p => p.Patients)
                .HasForeignKey(d => d.GenderCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("patients_gender_code_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Patients)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("patients_user_id_fkey");
        });

        modelBuilder.Entity<PrescribedMedication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prescribed_medications_pkey");

            entity.ToTable("prescribed_medications");

            entity.HasIndex(e => e.MedicationId, "IX_prescribed_medications_medication_id");

            entity.HasIndex(e => e.TreatmentPlanId, "idx_prescribed_medications_prescription_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Dosage)
                .HasMaxLength(100)
                .HasColumnName("dosage");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.Instructions).HasColumnName("instructions");
            entity.Property(e => e.MedicationId).HasColumnName("medication_id");
            entity.Property(e => e.TreatmentPlanId).HasColumnName("treatment_plan_id");

            entity.HasOne(d => d.Medication).WithMany(p => p.PrescribedMedications)
                .HasForeignKey(d => d.MedicationId)
                .HasConstraintName("prescribed_medications_medication_id_fkey");

            entity.HasOne(d => d.TreatmentPlan).WithMany(p => p.PrescribedMedications)
                .HasForeignKey(d => d.TreatmentPlanId)
                .HasConstraintName("prescribed_medications_treatment_plan_id_fkey");
        });

        modelBuilder.Entity<Registrator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("registrators_pkey");

            entity.ToTable("registrators");

            entity.HasIndex(e => e.GenderCode, "IX_registrators_gender_code");

            entity.HasIndex(e => e.Email, "registrators_email_key").IsUnique();

            entity.HasIndex(e => e.Phone, "registrators_phone_key").IsUnique();

            entity.HasIndex(e => e.UserId, "registrators_user_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.GenderCode).HasColumnName("gender_code");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(100)
                .HasColumnName("middle_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.GenderCodeNavigation).WithMany(p => p.Registrators)
                .HasForeignKey(d => d.GenderCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("registrators_gender_code_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.Registrator)
                .HasForeignKey<Registrator>(d => d.UserId)
                .HasConstraintName("registrators_user_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "roles_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TreatmentPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("treatment_plans_pkey");

            entity.ToTable("treatment_plans");

            entity.HasIndex(e => e.DoctorId, "idx_treatment_plans_doctor_id");

            entity.HasIndex(e => e.VisitId, "idx_treatment_plans_visit_id");

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

            entity.HasOne(d => d.Doctor).WithMany(p => p.TreatmentPlans)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("prescriptions_doctor_id_fkey");

            entity.HasOne(d => d.Visit).WithMany(p => p.TreatmentPlans)
                .HasForeignKey(d => d.VisitId)
                .HasConstraintName("prescriptions_visit_id_fkey");
        });

        modelBuilder.Entity<UnitsOfMeasurement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("units_of_measurement_pkey");

            entity.ToTable("units_of_measurement");

            entity.HasIndex(e => e.Symbol, "units_of_measurement_symbol_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Symbol)
                .HasMaxLength(20)
                .HasColumnName("symbol");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.RoleId, "idx_users_role_id");

            entity.HasIndex(e => e.Login, "users_login_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login)
                .HasMaxLength(255)
                .HasColumnName("login");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("users_role_id_fkey");
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("visits_pkey");

            entity.ToTable("visits");

            entity.HasIndex(e => e.DiagnosisId, "IX_visits_diagnosis_id");

            entity.HasIndex(e => e.AppointmentId, "idx_visits_appointment_id");

            entity.HasIndex(e => e.DoctorId, "idx_visits_doctor_id");

            entity.HasIndex(e => e.PatientId, "idx_visits_patient_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.ChiefComplaint).HasColumnName("chief_complaint");
            entity.Property(e => e.DiagnosisId).HasColumnName("diagnosis_id");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.VisitTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("visit_time");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Visits)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("visits_appointment_id_fkey");

            entity.HasOne(d => d.Diagnosis).WithMany(p => p.Visits)
                .HasForeignKey(d => d.DiagnosisId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("visits_diagnosis_id_fkey");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Visits)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("visits_doctor_id_fkey");

            entity.HasOne(d => d.Patient).WithMany(p => p.Visits)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("visits_patient_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
