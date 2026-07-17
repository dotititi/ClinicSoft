using ClinicSoft.Data;
using ClinicSoft.Views.DoctorViews;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows.Controls;

namespace ClinicSoft.Views.PatientViews
{
    public partial class MyAppointmentsPage : Page
    {
        private readonly int _patientUserId;
        private System.Collections.Generic.List<dynamic> _allAppointments = new();
        public MyAppointmentsPage(int patientUserId)
        {
            InitializeComponent();
            _patientUserId = patientUserId;
            LoadAppointments();
        }
        private void LoadAppointments()
        {
            using var context = new ClinicSoftContext();
            _allAppointments = context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.Patient.UserId == _patientUserId)
                .OrderByDescending(a => a.ScheduledTime)
                .Select(a => new
                {
                    a.Id,
                    a.ScheduledTime,
                    DoctorName = $"{a.Doctor.LastName} {a.Doctor.FirstName} {a.Doctor.MiddleName}",
                    DoctorFullName = $"{a.Doctor.LastName} {a.Doctor.FirstName} {a.Doctor.MiddleName}".Trim(),
                    StatusDisplay = GetStatusDisplayName(a.Status),
                    a.Reason
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
                AppointmentsGrid.ItemsSource = _allAppointments;
            }
            else
            {
                string searchLower = searchTerm.ToLowerInvariant();
                var filtered = _allAppointments
                    .Where(a => a.DoctorFullName?.ToString().ToLowerInvariant().Contains(searchLower) == true)
                    .ToList();
                AppointmentsGrid.ItemsSource = filtered;
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }
        private void BtnViewDetails_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int appointmentId)
            {
                var encounterPage = new EncounterPage(appointmentId, isDoctor: false);
                NavigationService?.Navigate(encounterPage);
            }
        }
        private static string GetStatusDisplayName(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "scheduled" => "Запланировано",
                "in_progress" => "В процессе",
                "completed" => "Завершено",
                "cancelled" => "Отменено",
                "no_show" => "Не явился",
                _ => "Неизвестно"
            };
        }
    }
}