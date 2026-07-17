using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class DoctorDashboardPage : Page
    {
        private readonly int _currentDoctorId;
        private string _currentFilter = "now";
        public DoctorDashboardPage(int currentDoctorId)
        {
            if (currentDoctorId <= 0)
                throw new ArgumentException("Некорректный ID врача", nameof(currentDoctorId));

            _currentDoctorId = currentDoctorId;
            InitializeComponent();
            Loaded += (s, e) => LoadData();
        }
        private void LoadData()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var now = DateTime.Now;
                var todayStart = now.Date;
                var todayEnd = todayStart.AddDays(1);
                var todaysAppointments = context.Appointments
                    .Count(a => a.DoctorId == _currentDoctorId &&
                                a.ScheduledTime >= todayStart &&
                                a.ScheduledTime < todayEnd);
                var completedToday = context.Appointments
                    .Count(a => a.DoctorId == _currentDoctorId &&
                                a.ScheduledTime >= todayStart &&
                                a.ScheduledTime < todayEnd &&
                                a.Status == "completed");
                IQueryable<Appointment> query;
                switch (_currentFilter)
                {
                    case "now":
                        var endOfWindow = now.AddMinutes(30);
                        query = context.Appointments
                            .Include(a => a.Patient)
                            .Where(a => a.DoctorId == _currentDoctorId &&
                                        a.ScheduledTime >= todayStart &&
                                        a.ScheduledTime < todayEnd &&
                                        a.ScheduledTime <= endOfWindow &&
                                        a.Status == "scheduled" &&
                                        a.Patient != null);
                        break;
                    case "today":
                        query = context.Appointments
                            .Include(a => a.Patient)
                            .Where(a => a.DoctorId == _currentDoctorId &&
                                        a.ScheduledTime >= todayStart &&
                                        a.ScheduledTime < todayEnd &&
                                        a.Status == "scheduled" &&
                                        a.Patient != null);
                        break;
                    case "completed":
                        query = context.Appointments
                            .Include(a => a.Patient)
                            .Where(a => a.DoctorId == _currentDoctorId &&
                                        a.ScheduledTime >= todayStart &&
                                        a.ScheduledTime < todayEnd &&
                                        a.Status == "completed" &&
                                        a.Patient != null);
                        break;
                    default:
                        query = context.Appointments
                            .Include(a => a.Patient)
                            .Where(a => a.DoctorId == _currentDoctorId &&
                                        a.ScheduledTime >= todayStart &&
                                        a.ScheduledTime < todayEnd &&
                                        a.Patient != null);
                        break;
                }
                string searchText = "";
                if (SearchBox != null)
                {
                    searchText = SearchBox.Text ?? "";
                    if (searchText == "Поиск по ФИО") searchText = "";
                }
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var term = searchText.Trim().ToLower();
                    query = query.Where(a =>
                        (a.Patient.LastName != null && a.Patient.LastName.ToLower().Contains(term)) ||
                        (a.Patient.FirstName != null && a.Patient.FirstName.ToLower().Contains(term)) ||
                        (a.Patient.MiddleName != null && a.Patient.MiddleName.ToLower().Contains(term)));
                }
                var appointments = query.OrderBy(a => a.ScheduledTime).ToList();
                var displayItems = appointments.Select(a => new
                {
                    Id = a.Id,
                    ScheduledTime = a.ScheduledTime,
                    PatientName = $"{a.Patient.LastName} {a.Patient.FirstName} {a.Patient.MiddleName}".Trim(),
                    Reason = a.Reason ?? "Без причины",
                    StatusText = GetStatusText(a),
                    ActionButtonText = _currentFilter == "completed" ? "Просмотреть" : "Начать приём",
                    ActionButtonColor = _currentFilter == "completed" ? "#4CAF50" : "#1976D2",
                    PrimaryButtonVisibility = (a.Status == "cancelled") ?
                        Visibility.Collapsed : Visibility.Visible,
                    CancelButtonVisibility = (a.Status == "scheduled" && _currentFilter != "completed") ?
                        Visibility.Visible : Visibility.Collapsed,
                    RowBackground = (a.Status == "cancelled") ? "#F5F5F5" : "White",
                    RowForeground = (a.Status == "cancelled") ? "#9E9E9E" : "#000000",
                    IsRowEnabled = (a.Status != "cancelled")
                }).ToList();
                if (IsLoaded)
                {
                    TodaysAppointmentsCount.Text = todaysAppointments.ToString();
                    CompletedTodayCount.Text = completedToday.ToString();
                    PendingAppointmentsGrid.ItemsSource = displayItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private string GetStatusText(Appointment appointment)
        {
            if (appointment.Status == "completed")
                return "Завершён";
            if (appointment.Status == "cancelled")
                return "Отменено";
            return appointment.ScheduledTime < DateTime.Now ? "Опоздал" : "Ожидает";
        }
        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag is int appointmentId)
                {
                    var encounterPage = new EncounterPage(appointmentId, isDoctor: true);
                    NavigationService?.Navigate(encounterPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        private void CancelAppointment_Click(object sender, RoutedEventArgs e)
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
                try
                {
                    using var context = new ClinicSoftContext();
                    var appointment = context.Appointments.Find(appointmentId);
                    if (appointment != null && appointment.Status == "scheduled")
                    {
                        appointment.Status = "cancelled";
                        context.SaveChanges();
                        LoadData();
                        MessageBox.Show("Запись отменена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отмены записи:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void TimeFilter_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            switch (radioButton?.Content.ToString())
            {
                case "Сейчас":
                    _currentFilter = "now";
                    break;
                case "Сегодня":
                    _currentFilter = "today";
                    break;
                case "Завершённые":
                    _currentFilter = "completed";
                    break;
            }
            LoadData();
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox == null || SearchPlaceholder == null) return;

            bool isEmpty = string.IsNullOrWhiteSpace(SearchBox.Text);
            SearchPlaceholder.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
            LoadData();
        }
    }
}