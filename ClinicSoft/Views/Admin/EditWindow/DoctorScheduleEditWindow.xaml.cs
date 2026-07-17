using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ClinicSoft.Views.Admin.EditWindow
{
    public partial class DoctorScheduleEditWindow : Window
    {
        private readonly int? _scheduleId;
        private readonly Dictionary<int, string> _daysOfWeek = new()
        {
            { 1, "Понедельник" },
            { 2, "Вторник" },
            { 3, "Среда" },
            { 4, "Четверг" },
            { 5, "Пятница" },
            { 6, "Суббота" },
            { 7, "Воскресенье" }
        };
        public DoctorScheduleEditWindow(int? scheduleId)
        {
            InitializeComponent();
            _scheduleId = scheduleId;
            CbDayOfWeek.ItemsSource = _daysOfWeek;
            LoadDoctors();
            if (scheduleId.HasValue)
            {
                TitleText.Text = "Редактировать расписание";
                LoadSchedule(scheduleId.Value);
            }
            else
            {
                TitleText.Text = "Добавить расписание";
                CbIsWorking.IsChecked = true;
                CbDayOfWeek.SelectedIndex = 0;
            }
        }
        private void LoadDoctors()
        {
            using var context = new ClinicSoftContext();
            var doctors = context.Doctors
                .Include(d => d.Speciality)
                .AsEnumerable()
                .Select(d => new
                {
                    Id = d.Id,
                    Display = $"{d.LastName} {d.FirstName} {(string.IsNullOrEmpty(d.MiddleName) ? "" : d.MiddleName)} ({(d.Speciality?.Name ?? "без специальности")})".Trim()
                })
                .OrderBy(d => d.Display)
                .ToList();
            CbDoctor.ItemsSource = doctors;
            CbDoctor.SelectedValuePath = "Id";
        }
        private void LoadSchedule(int scheduleId)
        {
            using var context = new ClinicSoftContext();
            var schedule = context.DoctorSchedules
                .Include(s => s.Doctor)
                .FirstOrDefault(s => s.Id == scheduleId);
            if (schedule != null)
            {
                CbDoctor.SelectedValue = schedule.DoctorId;
                CbDayOfWeek.SelectedValue = schedule.DayOfWeek;
                TxtStartTime.Text = schedule.StartTime.ToString("HH:mm");
                TxtEndTime.Text = schedule.EndTime.ToString("HH:mm");
                CbIsWorking.IsChecked = schedule.IsWorking;
            }
        }
        private void Time_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidateTimes();
        }
        private bool ValidateTimes()
        {
            string startTimeInput = TxtStartTime.Text.Trim().Replace('-', ':').Replace('.', ':');
            string endTimeInput = TxtEndTime.Text.Trim().Replace('-', ':').Replace('.', ':');
            if (!TimeOnly.TryParse(startTimeInput, out var start) ||
                !TimeOnly.TryParse(endTimeInput, out var end))
            {
                BtnSave.IsEnabled = false;
                return false;
            }
            if (end <= start)
            {
                BtnSave.IsEnabled = false;
                return false;
            }
            TxtStartTime.Text = start.ToString("HH:mm");
            TxtEndTime.Text = end.ToString("HH:mm");
            BtnSave.IsEnabled = true;
            return true;
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CbDoctor.SelectedValue == null || CbDayOfWeek.SelectedValue == null)
            {
                MessageBox.Show("Заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateTimes())
            {
                MessageBox.Show("Укажите корректное время начала и окончания (окончание должно быть позже начала).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                var doctorId = (int)CbDoctor.SelectedValue;
                var dayOfWeek = (int)CbDayOfWeek.SelectedValue;
                var startTime = TimeOnly.Parse(TxtStartTime.Text);
                var endTime = TimeOnly.Parse(TxtEndTime.Text);
                var isWorking = CbIsWorking.IsChecked == true;
                if (_scheduleId.HasValue)
                {
                    var schedule = context.DoctorSchedules.Find(_scheduleId.Value);
                    if (schedule != null)
                    {
                        schedule.DoctorId = doctorId;
                        schedule.DayOfWeek = dayOfWeek;
                        schedule.StartTime = startTime;
                        schedule.EndTime = endTime;
                        schedule.IsWorking = isWorking;
                    }
                }
                else
                {
                    var existing = context.DoctorSchedules
                        .FirstOrDefault(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek);
                    if (existing != null)
                    {
                        MessageBox.Show("Расписание для этого врача на выбранный день недели уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    context.DoctorSchedules.Add(new DoctorSchedule
                    {
                        DoctorId = doctorId,
                        DayOfWeek = dayOfWeek,
                        StartTime = startTime,
                        EndTime = endTime,
                        IsWorking = isWorking
                    });
                }
                context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}