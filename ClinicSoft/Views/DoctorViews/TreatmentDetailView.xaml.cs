using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class TreatmentDetailView : Page
    {
        public TreatmentDetailView(int prescriptionId)
        {
            InitializeComponent();
            LoadPrescriptionDetails(prescriptionId);
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
        }
        private void LoadPrescriptionDetails(int prescriptionId)
        {
            using var context = new ClinicSoftContext();
            var prescription = context.TreatmentPlans
                .Include(p => p.Visit)
                    .ThenInclude(v => v.Patient)
                .Include(p => p.PrescribedMedications)
                    .ThenInclude(pm => pm.Medication)
                .FirstOrDefault(p => p.Id == prescriptionId);
            if (prescription == null)
            {
                PatientNameText.Text = "План лечения не найден";
                return;
            }
            var patient = prescription.Visit.Patient;
            PatientNameText.Text = $"{patient.LastName} {patient.FirstName} {patient.MiddleName}".Trim();
            IssuedAtText.Text = prescription.IssuedAt.ToString("dd.MM.yyyy");
            StatusText.Text = TranslateStatus(prescription.Status);
            NotesText.Text = !string.IsNullOrWhiteSpace(prescription.Notes)
                ? prescription.Notes
                : "Без примечаний";
            var items = prescription.PrescribedMedications.Select(pm => new
            {
                Name = pm.Medication?.Name ?? "—",
                Dosage = pm.Dosage ?? "—",
                Duration = $"{pm.DurationDays} дн.",
                Instructions = pm.Instructions ?? "Инструкция не указана"
            }).ToList();
            ItemsGrid.ItemsSource = items;
        }
        private string TranslateStatus(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "completed" => "Завершён",
                "active" or null => "Активен",
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