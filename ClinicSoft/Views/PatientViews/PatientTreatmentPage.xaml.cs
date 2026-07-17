using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows.Controls;

namespace ClinicSoft.Views.PatientViews
{
    public partial class PatientTreatmentPage : Page
    {
        private readonly int _patientId;
        private System.Collections.Generic.List<dynamic> _allTreatments = new();

        public PatientTreatmentPage(int patientUserId)
        {
            InitializeComponent();
            _patientId = GetPatientIdByUserId(patientUserId);
            LoadTreatments();
        }
        private int GetPatientIdByUserId(int userId)
        {
            using var context = new ClinicSoftContext();
            return context.Patients
                .Where(p => p.UserId == userId)
                .Select(p => p.Id)
                .FirstOrDefault();
        }
        private void LoadTreatments()
        {
            using var context = new ClinicSoftContext();
            _allTreatments = context.TreatmentPlans
                .Include(p => p.Visit)
                    .ThenInclude(v => v.Patient)
                .Include(p => p.Doctor)
                .Where(p => p.Visit.PatientId == _patientId)
                .OrderByDescending(p => p.IssuedAt)
                .Select(p => new
                {
                    Id = p.Id,
                    IssuedAt = p.IssuedAt,
                    DoctorName = $"{p.Doctor.LastName} {p.Doctor.FirstName} {p.Doctor.MiddleName}",
                    DoctorFullName = $"{p.Doctor.LastName} {p.Doctor.FirstName} {p.Doctor.MiddleName}".Trim(),
                    Status = TranslateStatus(p.Status),
                    Notes = p.Notes ?? "Без примечаний"
                })
                .Cast<dynamic>()
                .ToList();
            ApplySearchFilter();
        }
        private void ApplySearchFilter()
        {
            string searchTerm = SearchBox?.Text?.Trim();
            bool hasSearch = !string.IsNullOrEmpty(searchTerm);
            SearchPlaceholder.Visibility = hasSearch ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            if (!hasSearch)
            {
                TreatmentsGrid.ItemsSource = _allTreatments;
            }
            else
            {
                string searchLower = searchTerm.ToLowerInvariant();
                var filtered = _allTreatments
                    .Where(t => t.DoctorFullName?.ToString().ToLowerInvariant().Contains(searchLower) == true)
                    .ToList();
                TreatmentsGrid.ItemsSource = filtered;
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }
        private static string TranslateStatus(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "completed" => "Завершён",
                "active" or null => "Активен",
                _ => status ?? "Неизвестен"
            };
        }
        private void BtnViewDetails_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is int prescriptionId)
            {
                NavigationService?.Navigate(new DoctorViews.TreatmentDetailView(prescriptionId));
            }
        }
    }
}