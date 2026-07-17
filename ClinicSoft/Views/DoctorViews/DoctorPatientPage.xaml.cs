using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class DoctorPatientPage : Page
    {
        private readonly int _doctorId;
        private string _searchTerm = "";
        private bool _showAllPatients;
        private class PatientItem
        {
            public int Id { get; set; }
            public string FullName { get; set; } = null!;
            public string FullSearchName { get; set; } = null!;
            public int Age { get; set; }
            public string Gender { get; set; } = null!;
            public string? Phone { get; set; }
            public string? Email { get; set; }
        }
        public DoctorPatientPage(int doctorId)
        {
            _doctorId = doctorId;
            _showAllPatients = false;
            InitializeComponent();
            SearchPlaceholder.Visibility = Visibility.Visible;
            LoadPatients();
        }
        public DoctorPatientPage(int doctorId, bool showAllPatients)
        {
            _doctorId = doctorId;
            _showAllPatients = showAllPatients;
            InitializeComponent();
            SearchPlaceholder.Visibility = Visibility.Visible;
            LoadPatients();
        }
        private void LoadPatients()
        {
            using var context = new ClinicSoftContext();
            IQueryable<Patient> baseQuery = context.Patients
                .Include(p => p.GenderCodeNavigation)
                .Include(p => p.MedicalCard);
            if (!_showAllPatients)
            {
                baseQuery = baseQuery.Where(p => p.Appointments.Any(a => a.DoctorId == _doctorId));
            }
            var allPatients = baseQuery.ToList();
            if (!string.IsNullOrWhiteSpace(_searchTerm))
            {
                string termLower = _searchTerm.Trim().ToLowerInvariant();
                allPatients = allPatients
                    .Where(p => !string.IsNullOrEmpty(p.LastName) &&
                               (
                                   p.LastName.ToLowerInvariant().Contains(termLower) ||
                                   p.FirstName.ToLowerInvariant().Contains(termLower) ||
                                   (p.MiddleName?.ToLowerInvariant().Contains(termLower) == true)
                               ))
                    .ToList();
            }
            var patients = allPatients
                .Select(p => new PatientItem
                {
                    Id = p.Id,
                    FullName = $"{p.LastName} {p.FirstName} {p.MiddleName}".Trim(),
                    FullSearchName = $"{p.LastName} {p.FirstName} {p.MiddleName}".Trim(),
                    Age = DateTime.Today.Year - p.Birthday.Year,
                    Gender = p.GenderCodeNavigation?.Name ?? "—",
                    Phone = p.Phone,
                    Email = p.Email
                })
                .ToList();
            PatientsGrid.ItemsSource = patients;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTerm = SearchBox.Text;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_searchTerm)
                ? Visibility.Visible
                : Visibility.Collapsed;
            LoadPatients();
        }
        private void PatientsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (PatientsGrid.SelectedItem is PatientItem patient)
            {
                NavigationService?.Navigate(new PatientCardPage(patient.Id));
            }
        }
        private void BtnViewCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int patientId)
            {
                NavigationService?.Navigate(new PatientCardPage(patientId));
            }
        }
    }
}