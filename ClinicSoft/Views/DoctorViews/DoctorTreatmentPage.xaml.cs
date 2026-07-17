using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class DoctorTreatmentPage : Page
    {
        private class PrescriptionItemView
        {
            public int Id { get; set; }
            public DateTime IssuedAt { get; set; }
            public string PatientName { get; set; } = null!;
            public string PatientFullName { get; set; } = null!;
            public string Status { get; set; } = null!;
            public string Notes { get; set; } = null!;
        }
        private readonly int _doctorId;
        private List<PrescriptionItemView> _allPrescriptions = new();
        public DoctorTreatmentPage(int doctorId)
        {
            InitializeComponent();
            _doctorId = doctorId;
            LoadPrescriptions();
        }
        private void LoadPrescriptions()
        {
            using var context = new ClinicSoftContext();
            _allPrescriptions = context.TreatmentPlans
                .Include(p => p.Visit)
                    .ThenInclude(v => v.Patient)
                .Where(p => p.DoctorId == _doctorId)
                .OrderByDescending(p => p.IssuedAt)
                .Select(p => new PrescriptionItemView
                {
                    Id = p.Id,
                    IssuedAt = p.IssuedAt,
                    PatientName = $"{p.Visit.Patient.LastName} {p.Visit.Patient.FirstName} {p.Visit.Patient.MiddleName}",
                    PatientFullName = $"{p.Visit.Patient.LastName} {p.Visit.Patient.FirstName} {p.Visit.Patient.MiddleName}".Trim(),
                    Status = TranslateStatus(p.Status),
                    Notes = p.Notes ?? "Без примечаний"
                })
                .ToList();
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
        private void ApplySearchFilter()
        {
            string searchTerm = SearchBox?.Text?.Trim();
            bool hasSearch = !string.IsNullOrEmpty(searchTerm);
            SearchPlaceholder.Visibility = hasSearch
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;
            if (!hasSearch)
            {
                PrescriptionsGrid.ItemsSource = _allPrescriptions;
            }
            else
            {
                searchTerm = searchTerm.ToLowerInvariant();
                var filtered = _allPrescriptions
                    .Where(p => p.PatientFullName.ToLowerInvariant().Contains(searchTerm))
                    .ToList();
                PrescriptionsGrid.ItemsSource = filtered;
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }
        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int prescriptionId)
            {
                NavigationService?.Navigate(new TreatmentDetailView(prescriptionId));
            }
        }
        private void PrescriptionsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (PrescriptionsGrid.SelectedItem is PrescriptionItemView selected)
            {
                NavigationService?.Navigate(new TreatmentDetailView(selected.Id));
            }
        }
    }
}