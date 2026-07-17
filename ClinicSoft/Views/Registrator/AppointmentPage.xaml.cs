using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ClinicSoft.Views.Registrator
{
    public partial class AppointmentPage : Page
    {
        private Dictionary<int, string> _doctorOffices = new();
        public AppointmentPage()
        {
            InitializeComponent();
            LoadData();
            SetInitialDate();
        }
        private void SetInitialDate()
        {
            var currentDate = DateTime.Today;
            while (IsWeekend(currentDate))
            {
                currentDate = currentDate.AddDays(1);
            }
            DpDate.SelectedDate = currentDate;
        }
        private void LoadData()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var patients = context.Patients
                    .AsEnumerable()
                    .Select(p => new
                    {
                        Id = p.Id,
                        Display = $"{p.LastName} {p.FirstName} {(string.IsNullOrEmpty(p.MiddleName) ? "" : p.MiddleName)}".Trim()
                    })
                    .OrderBy(p => p.Display)
                    .ToList();
                CbPatient.ItemsSource = patients;
                CbPatient.DisplayMemberPath = "Display";
                CbPatient.SelectedValuePath = "Id";
                var doctors = context.Doctors
                    .Where(d => d.StatusId == 1)
                    .Include(d => d.Speciality)
                    .Include(d => d.Office)
                    .AsEnumerable()
                    .ToList()
                    .Select(d => new
                    {
                        Id = d.Id,
                        Display = $"{d.LastName} {d.FirstName} {(string.IsNullOrEmpty(d.MiddleName) ? "" : d.MiddleName)} ({d.Speciality?.Name ?? "без специальности"})",
                        OfficeNumber = d.Office?.Number ?? "не указан"
                    })
                    .OrderBy(d => d.Display)
                    .ToList();
                _doctorOffices.Clear();
                foreach (var doc in doctors)
                {
                    _doctorOffices[doc.Id] = doc.OfficeNumber;
                }
                if (doctors.Any())
                {
                    CbDoctor.ItemsSource = doctors;
                    CbDoctor.DisplayMemberPath = "Display";
                    CbDoctor.SelectedValuePath = "Id";
                    CbDoctor.SelectedIndex = -1; 
                }
                else
                {
                    MessageBox.Show(
                        "Нет доступных врачей со статусом 'Активен'.\nОбратитесь к администратору.",
                        "Информация",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    CbDoctor.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ShowOfficeInfo(int doctorId)
        {
            if (_doctorOffices.TryGetValue(doctorId, out string officeNumber))
            {
                OfficeInfo.Text = $"Кабинет: {officeNumber}";
                OfficeInfo.Visibility = Visibility.Visible;
            }
            else
            {
                OfficeInfo.Visibility = Visibility.Collapsed;
            }
        }
        private void CbDoctor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CbTime.IsEnabled = false;
            CbTime.ItemsSource = new string[0];
            CbTime.SelectedIndex = -1;
            if (CbDoctor.SelectedValue is not null &&
                int.TryParse(CbDoctor.SelectedValue.ToString(), out int doctorId) &&
                doctorId != -1)
            {
                ShowOfficeInfo(doctorId);
                if (DpDate.SelectedDate.HasValue)
                {
                    var dateOnly = DateOnly.FromDateTime(DpDate.SelectedDate.Value);
                    LoadAvailableTimes(dateOnly, doctorId);
                }
            }
            else
            {
                OfficeInfo.Visibility = Visibility.Collapsed;
            }
        }
        private void DpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateErrorHint.Visibility = Visibility.Collapsed;
            if (!DpDate.SelectedDate.HasValue) return;
            if (IsWeekend(DpDate.SelectedDate.Value))
            {
                DateErrorHint.Visibility = Visibility.Visible;
                CbTime.ItemsSource = new string[0];
                CbTime.SelectedIndex = -1;
                return;
            }
            if (CbDoctor.SelectedValue is not null &&
                int.TryParse(CbDoctor.SelectedValue.ToString(), out int doctorId) &&
                doctorId != -1)
            {
                var dateOnly = DateOnly.FromDateTime(DpDate.SelectedDate.Value);
                LoadAvailableTimes(dateOnly, doctorId);
            }
            else
            {
                CbTime.ItemsSource = new string[0];
                CbTime.SelectedIndex = -1;
            }
        }
        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }
        private void LoadAvailableTimes(DateOnly date, int doctorId)
        {
            CbTime.IsEnabled = true;
            try
            {
                using var context = new ClinicSoftContext();
                var dayOfWeek = (int)date.ToDateTime(TimeOnly.MinValue).DayOfWeek;
                if (dayOfWeek == 0) dayOfWeek = 7;
                var schedule = context.DoctorSchedules
                    .FirstOrDefault(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek);
                if (schedule == null || !schedule.IsWorking)
                {
                    CbTime.ItemsSource = new string[0];
                    CbTime.SelectedIndex = -1;
                    TimeHint.Text = "Врач не работает в этот день";
                    return;
                }
                var slots = GenerateTimeSlots(schedule.StartTime, schedule.EndTime);
                var bookedTimes = context.Appointments
                    .Where(a => a.DoctorId == doctorId &&
                                DateOnly.FromDateTime(a.ScheduledTime.Date) == date)
                    .Select(a => a.ScheduledTime.ToString("HH:mm"))
                    .ToHashSet();
                var available = slots.Where(t => !bookedTimes.Contains(t)).ToList();
                CbTime.ItemsSource = available;
                CbTime.SelectedIndex = available.Any() ? 0 : -1;
                TimeHint.Text = available.Any()
                    ? $"Рабочее время: {schedule.StartTime:HH\\:mm}–{schedule.EndTime:HH\\:mm}"
                    : "Нет свободного времени";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки времени:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                CbTime.ItemsSource = new string[0];
                CbTime.SelectedIndex = -1;
                TimeHint.Text = "Ошибка загрузки расписания";
            }
        }
        private List<string> GenerateTimeSlots(TimeOnly start, TimeOnly end)
        {
            var slots = new List<string>();
            var current = start;
            while (current.Add(TimeSpan.FromMinutes(30)) <= end)
            {
                slots.Add(current.ToString("HH:mm"));
                current = current.Add(TimeSpan.FromMinutes(30));
            }
            return slots;
        }
        private void BtnCreateAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (!DpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату приёма.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (IsWeekend(DpDate.SelectedDate.Value))
            {
                MessageBox.Show("Запись недоступна в выходные дни.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CbPatient.SelectedValue == null)
            {
                MessageBox.Show("Выберите пациента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CbDoctor.SelectedIndex == -1 || CbDoctor.SelectedValue == null)
            {
                MessageBox.Show("Выберите врача.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CbTime.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите время приёма.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtReason.Text))
            {
                MessageBox.Show("Укажите причину обращения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                int patientId = (int)CbPatient.SelectedValue;
                int doctorId = (int)CbDoctor.SelectedValue;
                var timeStr = (string)CbTime.SelectedItem;
                var dateTime = DpDate.SelectedDate.Value.Date + TimeSpan.Parse(timeStr);
                if (context.Appointments.Any(a =>
                    a.DoctorId == doctorId &&
                    a.ScheduledTime.Date == dateTime.Date &&
                    a.ScheduledTime.Hour == dateTime.Hour &&
                    a.ScheduledTime.Minute == dateTime.Minute))
                {
                    MessageBox.Show("Это время уже занято.", "Конфликт", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                context.Appointments.Add(new Appointment
                {
                    PatientId = patientId,
                    DoctorId = doctorId,
                    ScheduledTime = dateTime,
                    Reason = TxtReason.Text.Trim(),
                    Status = "scheduled",
                    CreatedAt = DateTime.Now
                });
                context.SaveChanges();
                MessageBox.Show(
                    $"Пациент успешно записан!\n{CbPatient.Text}\n{CbDoctor.Text}\n{dateTime:dd.MM.yyyy HH:mm}",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                CbPatient.SelectedIndex = -1;
                CbDoctor.SelectedIndex = -1;
                TxtReason.Clear();
                OfficeInfo.Visibility = Visibility.Collapsed;
                SetInitialDate();
                CbTime.ItemsSource = new string[0];
                CbTime.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}