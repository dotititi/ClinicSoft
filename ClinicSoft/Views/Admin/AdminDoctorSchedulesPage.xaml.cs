using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminDoctorSchedulesPage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 520;
        private DoctorScheduleDisplayModel? _selectedSchedule;
        private string _currentSearchText = "";
        public AdminDoctorSchedulesPage()
        {
            InitializeComponent();
            LoadSchedules();
        }
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth < MIN_WIDTH_FOR_SINGLE_LINE)
            {
                SingleLineToolbar.Visibility = Visibility.Collapsed;
                MultiLineToolbar.Visibility = Visibility.Visible;
            }
            else
            {
                SingleLineToolbar.Visibility = Visibility.Visible;
                MultiLineToolbar.Visibility = Visibility.Collapsed;
            }
            UpdateSearchDisplay();
        }
        private void LoadSchedules(string searchTerm = "")
        {
            try
            {
                using var context = new ClinicSoftContext();
                var schedules = context.DoctorSchedules
                    .Include(s => s.Doctor)
                        .ThenInclude(d => d.Speciality)
                    .AsEnumerable()
                    .Select(s => new DoctorScheduleDisplayModel
                    {
                        Id = s.Id,
                        DoctorId = s.DoctorId,
                        DoctorName = $"{s.Doctor.LastName} {s.Doctor.FirstName} {(string.IsNullOrEmpty(s.Doctor.MiddleName) ? "" : s.Doctor.MiddleName)}".Trim(),
                        DayNumber = s.DayOfWeek,
                        DayOfWeek = GetDayName(s.DayOfWeek),
                        StartTime = s.StartTime.ToString("HH:mm"),
                        EndTime = s.EndTime.ToString("HH:mm"),
                        IsWorkingDisplay = s.IsWorking ? "Да" : "Нет",
                        IsWorking = s.IsWorking
                    })
                    .OrderBy(s => s.DoctorName)
                    .ThenBy(s => s.DayNumber)
                    .ToList();
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    string term = searchTerm.Trim().ToLowerInvariant();
                    schedules = schedules.Where(s =>
                        s.DoctorName.ToLowerInvariant().Contains(term) ||
                        s.DayOfWeek.ToLowerInvariant().Contains(term)
                    ).ToList();
                }
                DgSchedules.ItemsSource = schedules;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки расписания:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private string GetDayName(int dayNumber)
        {
            return dayNumber switch
            {
                1 => "Понедельник",
                2 => "Вторник",
                3 => "Среда",
                4 => "Четверг",
                5 => "Пятница",
                6 => "Суббота",
                7 => "Воскресенье",
                _ => "Неизвестно"
            };
        }
        private void DgSchedules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSchedule = DgSchedules.SelectedItem as DoctorScheduleDisplayModel;
            bool hasSelection = _selectedSchedule != null;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow.DoctorScheduleEditWindow(null);
            if (editWindow.ShowDialog() == true)
            {
                LoadSchedules(_currentSearchText);
            }
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSchedule != null)
            {
                var editWindow = new EditWindow.DoctorScheduleEditWindow(_selectedSchedule.Id);
                if (editWindow.ShowDialog() == true)
                {
                    LoadSchedules(_currentSearchText);
                }
            }
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSchedule != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить расписание для врача {_selectedSchedule.DoctorName} на {GetDayName(_selectedSchedule.DayNumber)}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var context = new ClinicSoftContext();
                        var schedule = context.DoctorSchedules.Find(_selectedSchedule.Id);
                        if (schedule != null)
                        {
                            context.DoctorSchedules.Remove(schedule);
                            context.SaveChanges();
                            LoadSchedules(_currentSearchText);
                            MessageBox.Show("Расписание успешно удалено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadSchedules(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        public class DoctorScheduleDisplayModel
        {
            public int Id { get; set; }
            public string DoctorName { get; set; } = string.Empty;
            public string DayOfWeek { get; set; } = string.Empty;
            public string StartTime { get; set; } = string.Empty;
            public string EndTime { get; set; } = string.Empty;
            public string IsWorkingDisplay { get; set; } = "Нет";
            public bool IsWorking { get; set; }
            public int DoctorId { get; set; }
            public int DayNumber { get; set; }
        }
    }
}