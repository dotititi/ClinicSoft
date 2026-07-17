using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.PatientViews
{
    public partial class BookAppointmentPage : Page
    {
        private readonly int _patientUserId;
        private Dictionary<int, string> _doctorOffices = new();
        public BookAppointmentPage(int patientUserId)
        {
            InitializeComponent();
            _patientUserId = patientUserId;
            SetInitialDate();
            LoadDoctors();
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
        private void LoadDoctors()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var doctors = context.Doctors
                    .Where(d => d.StatusId == 1)
                    .Include(d => d.Speciality)
                    .Include(d => d.Office)
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
                    CbDoctor.SelectedValuePath = "Id";
                    CbDoctor.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show(
                        "Нет доступных врачей для записи.\nОбратитесь к администратору.",
                        "Информация",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    CbDoctor.IsEnabled = false;
                    BtnBook.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки врачей:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                CbDoctor.SelectedValue is var value &&
                value.ToString() != "-1" &&
                int.TryParse(value.ToString(), out int doctorId))
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
            if (!DpDate.SelectedDate.HasValue)
                return;
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
        private void BtnBook_Click(object sender, RoutedEventArgs e)
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
                var patient = context.Patients.FirstOrDefault(p => p.UserId == _patientUserId);
                if (patient == null)
                {
                    MessageBox.Show("Ваш профиль не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var doctorId = (int)CbDoctor.SelectedValue;
                var timeStr = (string)CbTime.SelectedItem;
                var dateTime = DpDate.SelectedDate.Value.Date + TimeSpan.Parse(timeStr);
                if (context.Appointments.Any(a =>
                    a.DoctorId == doctorId &&
                    a.ScheduledTime.Date == dateTime.Date &&
                    a.ScheduledTime.Hour == dateTime.Hour &&
                    a.ScheduledTime.Minute == dateTime.Minute))
                {
                    MessageBox.Show("Это время уже занято. Обновите список.", "Конфликт", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                context.Appointments.Add(new Appointment
                {
                    PatientId = patient.Id,
                    DoctorId = doctorId,
                    ScheduledTime = dateTime,
                    Reason = TxtReason.Text.Trim(),
                    Status = "scheduled",
                    CreatedAt = DateTime.Now
                });
                context.SaveChanges();
                MessageBox.Show(
                    $"Вы успешно записаны!\n\nВрач: {CbDoctor.Text}\nДата: {dateTime:dd.MM.yyyy}\nВремя: {dateTime:HH:mm}",
                    "Успешно",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}