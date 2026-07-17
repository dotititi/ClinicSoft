using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class DoctorAppointmentPage : Page
    {
        private readonly int _doctorId;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _searchTerm = "";
        private class AppointmentItem
        {
            public int Id { get; set; }
            public DateTime ScheduledTime { get; set; }
            public string PatientName { get; set; } = null!;
            public string Status { get; set; } = null!;
            public string StatusDisplay { get; set; } = null!;
            public string? Reason { get; set; }
            public System.Windows.Visibility CancelButtonVisibility =>
                Status == "scheduled" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            public System.Windows.Visibility OpenButtonVisibility =>
                Status == "cancelled" ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            public string RowBackground => Status == "cancelled" ? "#F5F5F5" : "White";
            public string RowForeground => Status == "cancelled" ? "#9E9E9E" : "#000000";
            public bool IsRowEnabled => Status != "cancelled";
        }
        public DoctorAppointmentPage(int doctorId)
        {
            _doctorId = doctorId;
            InitializeComponent();
            StartDatePicker.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            EndDatePicker.SelectedDate = StartDatePicker.SelectedDate.Value.AddMonths(1).AddDays(-1);
            Loaded += (s, e) => LoadAppointments();
        }
        private void LoadAppointments()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var baseQuery = context.Appointments
                    .Include(a => a.Patient)
                    .Where(a => a.DoctorId == _doctorId && a.Patient != null);
                if (_startDate.HasValue)
                    baseQuery = baseQuery.Where(a => a.ScheduledTime >= _startDate.Value.Date);
                if (_endDate.HasValue)
                    baseQuery = baseQuery.Where(a => a.ScheduledTime < _endDate.Value.Date.AddDays(1));
                var allAppointments = baseQuery.ToList();
                if (!string.IsNullOrWhiteSpace(_searchTerm))
                {
                    string termLower = _searchTerm.Trim().ToLowerInvariant();
                    allAppointments = allAppointments
                        .Where(a => a.Patient != null &&
                                   (
                                       (a.Patient.LastName?.ToLowerInvariant().Contains(termLower) == true) ||
                                       (a.Patient.FirstName?.ToLowerInvariant().Contains(termLower) == true) ||
                                       (a.Patient.MiddleName?.ToLowerInvariant().Contains(termLower) == true)
                                   ))
                        .ToList();
                }
                var appointments = allAppointments
                    .OrderByDescending(a => a.ScheduledTime)
                    .Select(a => new AppointmentItem
                    {
                        Id = a.Id,
                        ScheduledTime = a.ScheduledTime,
                        PatientName = $"{a.Patient.LastName} {a.Patient.FirstName} {a.Patient.MiddleName}".Trim(),
                        Status = a.Status ?? "unknown",
                        StatusDisplay = GetStatusDisplayName(a.Status),
                        Reason = a.Reason
                    })
                    .ToList();
                if (IsLoaded)
                {
                    AppointmentsGrid.ItemsSource = appointments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки записей:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker == null || EndDatePicker == null) return;
            _startDate = StartDatePicker.SelectedDate;
            _endDate = EndDatePicker.SelectedDate;
            LoadAppointments();
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(SearchBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
            _searchTerm = SearchBox.Text ?? "";
            LoadAppointments();
        }
        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var appointmentId = (int)button.Tag;
            NavigationService?.Navigate(new EncounterPage(appointmentId, isDoctor: true));
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var appointmentId = (int)button.Tag;
            var result = MessageBox.Show(
                "Вы уверены, что хотите отменить запись?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                using var context = new ClinicSoftContext();
                var appointment = context.Appointments.Find(appointmentId);
                if (appointment != null && appointment.Status == "scheduled")
                {
                    appointment.Status = "cancelled";
                    context.SaveChanges();
                    LoadAppointments();
                    MessageBox.Show("Запись отменена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}