using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class PatientCardPage : Page
    {
        private readonly int _patientId;
        public PatientCardPage(int patientId)
        {
            InitializeComponent();
            _patientId = patientId;
            LoadPatientData();
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
        }
        private void LoadPatientData()
        {
            using var context = new ClinicSoftContext();
            var patient = context.Patients
                .Include(p => p.MedicalCard)
                .Include(p => p.GenderCodeNavigation)
                .First(p => p.Id == _patientId);
            PatientFullName.Text = $"{patient.LastName} {patient.FirstName} {patient.MiddleName}".Trim();
            var age = DateTime.Today.Year - patient.Birthday.Year;
            PatientInfo.Text = $"{age} лет, {patient.GenderCodeNavigation?.Name ?? "пол не указан"}";
            PhoneText.Text = patient.Phone ?? "Не указан";
            EmailText.Text = patient.Email ?? "Не указан";
            InsuranceText.Text = patient.MedicalCard?.InsuranceNumber ?? "Нет";
            BirthdayText.Text = patient.Birthday.ToString("dd.MM.yyyy");
            var allergies = patient.MedicalCard?.Allergies?.Trim() ?? "Нет";
            var chronic = patient.MedicalCard?.ChronicConditions?.Trim() ?? "Нет";
            MedicalHistoryText.Text = $"Аллергии: {allergies}\nХронические болезни: {chronic}";
            var visits = context.Visits
                .Where(v => v.PatientId == _patientId)
                .OrderByDescending(v => v.VisitTime)
                .Select(v => new
                {
                    v.Id,
                    v.VisitTime,
                    v.ChiefComplaint,
                    Status = "Завершён"
                })
                .ToList();
            VisitsGrid.ItemsSource = visits;
            var labOrders = context.LabOrders
                .Include(lo => lo.LabOrderItems)
                    .ThenInclude(loi => loi.TestType)
                .Where(lo => lo.PatientId == _patientId)
                .OrderByDescending(lo => lo.OrderedAt)
                .Select(lo => new
                {
                    lo.OrderedAt,
                    TestTypes = string.Join(", ", lo.LabOrderItems.Select(loi => loi.TestType.Name)),
                    Status = lo.Status,
                    StatusDisplay = TranslateLabOrderStatus(lo.Status)
                })
                .ToList();
            LabOrdersGrid.ItemsSource = labOrders;
            var diagnoses = context.Visits
                .Include(v => v.Diagnosis)
                .Where(v => v.PatientId == _patientId && v.Diagnosis != null)
                .OrderByDescending(v => v.VisitTime)
                .Select(v => new
                {
                    v.VisitTime,
                    DiagnosisName = v.Diagnosis!.Name
                })
                .ToList();
            DiagnosesGrid.ItemsSource = diagnoses;
            var medications = context.PrescribedMedications
                .Include(pm => pm.Medication)
                .Include(pm => pm.TreatmentPlan)
                    .ThenInclude(p => p.Visit)
                .Where(pm => pm.TreatmentPlan.Visit.PatientId == _patientId)
                .OrderByDescending(pm => pm.TreatmentPlan.IssuedAt)
                .Select(pm => new
                {
                    MedicationName = pm.Medication.Name,
                    pm.Dosage,
                    pm.DurationDays,
                    Instructions = pm.Instructions ?? "Инструкция не указана"
                })
                .ToList();
            MedicationsGrid.ItemsSource = medications;
        }
        private static string TranslateLabOrderStatus(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "pending" => "Ожидает",
                "in_progress" => "В работе",
                "completed" => "Завершён",
                "cancelled" => "Отменён",
                _ => status ?? "Неизвестен"
            };
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && NavigationService?.CanGoBack == true)
            {
                GoBack();
                e.Handled = true;
            }
        }
        private void GoBack()
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Нет страниц для возврата.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}