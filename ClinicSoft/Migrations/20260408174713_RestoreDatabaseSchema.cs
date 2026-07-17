using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClinicSoft.Migrations
{
    /// <inheritdoc />
    public partial class RestoreDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("departments_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "diagnoses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("diagnoses_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "doctor_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("doctor_statuses_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("document_types_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dosage_forms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("dosage_forms_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "genders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("genders_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "medical_specialities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("medical_specialities_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("offices_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("roles_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "units_of_measurement",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("units_of_measurement_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    document_type_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("document_templates_pkey", x => x.id);
                    table.ForeignKey(
                        name: "document_templates_document_type_id_fkey",
                        column: x => x.document_type_id,
                        principalTable: "document_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "medications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    dosage_form_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("medications_pkey", x => x.id);
                    table.ForeignKey(
                        name: "medications_dosage_form_id_fkey",
                        column: x => x.dosage_form_id,
                        principalTable: "dosage_forms",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    login = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                    table.ForeignKey(
                        name: "users_role_id_fkey",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lab_test_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    normal_range = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    unit_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("lab_test_types_pkey", x => x.id);
                    table.ForeignKey(
                        name: "lab_test_types_unit_id_fkey",
                        column: x => x.unit_id,
                        principalTable: "units_of_measurement",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    birthday = table.Column<DateOnly>(type: "date", nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    gender_code = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("admins_pkey", x => x.id);
                    table.ForeignKey(
                        name: "admins_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_admin_gender",
                        column: x => x.gender_code,
                        principalTable: "genders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "doctors",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    speciality_id = table.Column<int>(type: "integer", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    office_id = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    birthday = table.Column<DateOnly>(type: "date", nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    gender_code = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("doctors_pkey", x => x.id);
                    table.ForeignKey(
                        name: "doctors_department_id_fkey",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "doctors_gender_code_fkey",
                        column: x => x.gender_code,
                        principalTable: "genders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "doctors_office_id_fkey",
                        column: x => x.office_id,
                        principalTable: "offices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "doctors_speciality_id_fkey",
                        column: x => x.speciality_id,
                        principalTable: "medical_specialities",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "doctors_status_id_fkey",
                        column: x => x.status_id,
                        principalTable: "doctor_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "doctors_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patients",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    birthday = table.Column<DateOnly>(type: "date", nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    gender_code = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("patients_pkey", x => x.id);
                    table.ForeignKey(
                        name: "patients_gender_code_fkey",
                        column: x => x.gender_code,
                        principalTable: "genders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "patients_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "registrators",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    birthday = table.Column<DateOnly>(type: "date", nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    gender_code = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("registrators_pkey", x => x.id);
                    table.ForeignKey(
                        name: "registrators_gender_code_fkey",
                        column: x => x.gender_code,
                        principalTable: "genders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "registrators_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "appointments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    patient_id = table.Column<int>(type: "integer", nullable: false),
                    doctor_id = table.Column<int>(type: "integer", nullable: false),
                    scheduled_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'scheduled'::character varying"),
                    reason = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("appointments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "appointments_doctor_id_fkey",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "appointments_patient_id_fkey",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    patient_id = table.Column<int>(type: "integer", nullable: false),
                    doctor_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    document_template_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("documents_pkey", x => x.id);
                    table.ForeignKey(
                        name: "documents_doctor_id_fkey",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "documents_document_template_id_fkey",
                        column: x => x.document_template_id,
                        principalTable: "document_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "documents_patient_id_fkey",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "medical_cards",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    patient_id = table.Column<int>(type: "integer", nullable: false),
                    insurance_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    chronic_conditions = table.Column<string>(type: "text", nullable: false),
                    allergies = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("medical_cards_pkey", x => x.id);
                    table.ForeignKey(
                        name: "medical_cards_patient_id_fkey",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visits",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    appointment_id = table.Column<int>(type: "integer", nullable: false),
                    patient_id = table.Column<int>(type: "integer", nullable: false),
                    doctor_id = table.Column<int>(type: "integer", nullable: false),
                    visit_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    chief_complaint = table.Column<string>(type: "text", nullable: false),
                    diagnosis_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("visits_pkey", x => x.id);
                    table.ForeignKey(
                        name: "visits_appointment_id_fkey",
                        column: x => x.appointment_id,
                        principalTable: "appointments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "visits_diagnosis_id_fkey",
                        column: x => x.diagnosis_id,
                        principalTable: "diagnoses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "visits_doctor_id_fkey",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "visits_patient_id_fkey",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lab_orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    visit_id = table.Column<int>(type: "integer", nullable: false),
                    doctor_id = table.Column<int>(type: "integer", nullable: false),
                    patient_id = table.Column<int>(type: "integer", nullable: false),
                    ordered_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'pending'::character varying"),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("lab_orders_pkey", x => x.id);
                    table.ForeignKey(
                        name: "lab_orders_doctor_id_fkey",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "lab_orders_patient_id_fkey",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "lab_orders_visit_id_fkey",
                        column: x => x.visit_id,
                        principalTable: "visits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prescriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    visit_id = table.Column<int>(type: "integer", nullable: false),
                    doctor_id = table.Column<int>(type: "integer", nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'active'::character varying"),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("prescriptions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "prescriptions_doctor_id_fkey",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "prescriptions_visit_id_fkey",
                        column: x => x.visit_id,
                        principalTable: "visits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lab_order_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lab_order_id = table.Column<int>(type: "integer", nullable: false),
                    test_type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("lab_order_items_pkey", x => x.id);
                    table.ForeignKey(
                        name: "lab_order_items_lab_order_id_fkey",
                        column: x => x.lab_order_id,
                        principalTable: "lab_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "lab_order_items_test_type_id_fkey",
                        column: x => x.test_type_id,
                        principalTable: "lab_test_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lab_results",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lab_order_id = table.Column<int>(type: "integer", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    performed_by = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("lab_results_pkey", x => x.id);
                    table.ForeignKey(
                        name: "lab_results_lab_order_id_fkey",
                        column: x => x.lab_order_id,
                        principalTable: "lab_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "lab_results_performed_by_fkey",
                        column: x => x.performed_by,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prescribed_medications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prescription_id = table.Column<int>(type: "integer", nullable: false),
                    medication_id = table.Column<int>(type: "integer", nullable: false),
                    dosage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    duration_days = table.Column<int>(type: "integer", nullable: false),
                    instructions = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("prescribed_medications_pkey", x => x.id);
                    table.ForeignKey(
                        name: "prescribed_medications_medication_id_fkey",
                        column: x => x.medication_id,
                        principalTable: "medications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "prescribed_medications_prescription_id_fkey",
                        column: x => x.prescription_id,
                        principalTable: "prescriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lab_result_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lab_result_id = table.Column<int>(type: "integer", nullable: false),
                    test_type_id = table.Column<int>(type: "integer", nullable: false),
                    result_value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("lab_result_items_pkey", x => x.id);
                    table.ForeignKey(
                        name: "lab_result_items_lab_result_id_fkey",
                        column: x => x.lab_result_id,
                        principalTable: "lab_results",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "lab_result_items_test_type_id_fkey",
                        column: x => x.test_type_id,
                        principalTable: "lab_test_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "admins_email_key",
                table: "admins",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "admins_phone_key",
                table: "admins",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "admins_user_id_key",
                table: "admins",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admins_gender_code",
                table: "admins",
                column: "gender_code");

            migrationBuilder.CreateIndex(
                name: "appointments_doctor_time_key",
                table: "appointments",
                columns: new[] { "doctor_id", "scheduled_time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_appointments_patient_doctor",
                table: "appointments",
                columns: new[] { "patient_id", "doctor_id" });

            migrationBuilder.CreateIndex(
                name: "idx_appointments_scheduled_time",
                table: "appointments",
                column: "scheduled_time");

            migrationBuilder.CreateIndex(
                name: "departments_name_key",
                table: "departments",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "diagnoses_name_key",
                table: "diagnoses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "doctor_statuses_name_key",
                table: "doctor_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "doctors_phone_key",
                table: "doctors",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_doctors_email",
                table: "doctors",
                column: "email",
                unique: true,
                filter: "((email IS NOT NULL) AND ((email)::text <> ''::text))");

            migrationBuilder.CreateIndex(
                name: "idx_doctors_user_id",
                table: "doctors",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctors_department_id",
                table: "doctors",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctors_gender_code",
                table: "doctors",
                column: "gender_code");

            migrationBuilder.CreateIndex(
                name: "IX_doctors_office_id",
                table: "doctors",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctors_speciality_id",
                table: "doctors",
                column: "speciality_id");

            migrationBuilder.CreateIndex(
                name: "IX_doctors_status_id",
                table: "doctors",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "document_templates_name_key",
                table: "document_templates",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_templates_document_type_id",
                table: "document_templates",
                column: "document_type_id");

            migrationBuilder.CreateIndex(
                name: "document_types_name_key",
                table: "document_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_documents_created_at",
                table: "documents",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_documents_doctor_id",
                table: "documents",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "idx_documents_patient_id",
                table: "documents",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_documents_document_template_id",
                table: "documents",
                column: "document_template_id");

            migrationBuilder.CreateIndex(
                name: "dosage_forms_name_key",
                table: "dosage_forms",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "genders_name_key",
                table: "genders",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_lab_order_items_lab_order_id",
                table: "lab_order_items",
                column: "lab_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_order_items_test_type_id",
                table: "lab_order_items",
                column: "test_type_id");

            migrationBuilder.CreateIndex(
                name: "lab_order_items_order_test_key",
                table: "lab_order_items",
                columns: new[] { "lab_order_id", "test_type_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_lab_orders_doctor_id",
                table: "lab_orders",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "idx_lab_orders_patient_id",
                table: "lab_orders",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "idx_lab_orders_visit_id",
                table: "lab_orders",
                column: "visit_id");

            migrationBuilder.CreateIndex(
                name: "idx_lab_result_items_lab_result_id",
                table: "lab_result_items",
                column: "lab_result_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_result_items_test_type_id",
                table: "lab_result_items",
                column: "test_type_id");

            migrationBuilder.CreateIndex(
                name: "lab_result_items_result_test_key",
                table: "lab_result_items",
                columns: new[] { "lab_result_id", "test_type_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_lab_results_performed_by",
                table: "lab_results",
                column: "performed_by");

            migrationBuilder.CreateIndex(
                name: "lab_results_lab_order_id_key",
                table: "lab_results",
                column: "lab_order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_lab_test_types_name",
                table: "lab_test_types",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_lab_test_types_unit_id",
                table: "lab_test_types",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "lab_test_types_name_key",
                table: "lab_test_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_medical_cards_insurance_number",
                table: "medical_cards",
                column: "insurance_number");

            migrationBuilder.CreateIndex(
                name: "medical_cards_insurance_number_key",
                table: "medical_cards",
                column: "insurance_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "medical_cards_patient_id_key",
                table: "medical_cards",
                column: "patient_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "medical_specialities_name_key",
                table: "medical_specialities",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_medications_name",
                table: "medications",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_medications_dosage_form_id",
                table: "medications",
                column: "dosage_form_id");

            migrationBuilder.CreateIndex(
                name: "medications_name_dosage_form_key",
                table: "medications",
                columns: new[] { "name", "dosage_form_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "offices_number_key",
                table: "offices",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_patients_user_id",
                table: "patients",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_patients_gender_code",
                table: "patients",
                column: "gender_code");

            migrationBuilder.CreateIndex(
                name: "patients_email_key",
                table: "patients",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "patients_phone_key",
                table: "patients",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_prescribed_medications_prescription_id",
                table: "prescribed_medications",
                column: "prescription_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescribed_medications_medication_id",
                table: "prescribed_medications",
                column: "medication_id");

            migrationBuilder.CreateIndex(
                name: "idx_prescriptions_doctor_id",
                table: "prescriptions",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "idx_prescriptions_visit_id",
                table: "prescriptions",
                column: "visit_id");

            migrationBuilder.CreateIndex(
                name: "IX_registrators_gender_code",
                table: "registrators",
                column: "gender_code");

            migrationBuilder.CreateIndex(
                name: "registrators_email_key",
                table: "registrators",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "registrators_phone_key",
                table: "registrators",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "registrators_user_id_key",
                table: "registrators",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "roles_name_key",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "units_of_measurement_symbol_key",
                table: "units_of_measurement",
                column: "symbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "users_login_key",
                table: "users",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_visits_appointment_id",
                table: "visits",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "idx_visits_doctor_id",
                table: "visits",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "idx_visits_patient_id",
                table: "visits",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_visits_diagnosis_id",
                table: "visits",
                column: "diagnosis_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "lab_order_items");

            migrationBuilder.DropTable(
                name: "lab_result_items");

            migrationBuilder.DropTable(
                name: "medical_cards");

            migrationBuilder.DropTable(
                name: "prescribed_medications");

            migrationBuilder.DropTable(
                name: "registrators");

            migrationBuilder.DropTable(
                name: "document_templates");

            migrationBuilder.DropTable(
                name: "lab_results");

            migrationBuilder.DropTable(
                name: "lab_test_types");

            migrationBuilder.DropTable(
                name: "medications");

            migrationBuilder.DropTable(
                name: "prescriptions");

            migrationBuilder.DropTable(
                name: "document_types");

            migrationBuilder.DropTable(
                name: "lab_orders");

            migrationBuilder.DropTable(
                name: "units_of_measurement");

            migrationBuilder.DropTable(
                name: "dosage_forms");

            migrationBuilder.DropTable(
                name: "visits");

            migrationBuilder.DropTable(
                name: "appointments");

            migrationBuilder.DropTable(
                name: "diagnoses");

            migrationBuilder.DropTable(
                name: "doctors");

            migrationBuilder.DropTable(
                name: "patients");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "offices");

            migrationBuilder.DropTable(
                name: "medical_specialities");

            migrationBuilder.DropTable(
                name: "doctor_statuses");

            migrationBuilder.DropTable(
                name: "genders");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
