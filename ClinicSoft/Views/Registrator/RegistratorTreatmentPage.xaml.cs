using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSoft.Views.Registrator
{
    public partial class RegistratorTreatmentPage : Page
    {
        private class TreatmentItemView
        {
            public int Id { get; set; }
            public DateTime IssuedAt { get; set; }
            public string PatientName { get; set; } = null!;
            public string PatientFullName { get; set; } = null!;
            public string DoctorName { get; set; } = null!;
            public string Status { get; set; } = null!;
            public string Notes { get; set; } = null!;
        }
        private readonly int? _patientIdFilter;
        private List<TreatmentItemView> _allTreatments = new();
        public RegistratorTreatmentPage()
        {
            InitializeComponent();
            LoadTreatments();
            UpdateSearchPlaceholder();
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
        }
        public RegistratorTreatmentPage(int patientId)
        {
            InitializeComponent();
            _patientIdFilter = patientId;
            LoadTreatments();
            UpdateSearchPlaceholder();
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
        }
        private void LoadTreatments()
        {
            using var context = new ClinicSoftContext();
            IQueryable<TreatmentPlan> query = context.TreatmentPlans
                .Include(p => p.Visit)
                    .ThenInclude(v => v.Patient)
                .Include(p => p.Doctor);
            if (_patientIdFilter.HasValue)
            {
                query = query.Where(p => p.Visit.PatientId == _patientIdFilter.Value);
            }
            _allTreatments = query
                .Select(p => new TreatmentItemView
                {
                    Id = p.Id,
                    IssuedAt = p.IssuedAt,
                    PatientName = $"{p.Visit.Patient.LastName} {p.Visit.Patient.FirstName} {p.Visit.Patient.MiddleName}",
                    PatientFullName = $"{p.Visit.Patient.LastName} {p.Visit.Patient.FirstName} {p.Visit.Patient.MiddleName}".Trim(),
                    DoctorName = $"{p.Doctor.LastName} {p.Doctor.FirstName} {p.Doctor.MiddleName}",
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
        private void UpdateSearchPlaceholder()
        {
            if (_patientIdFilter.HasValue)
            {
                SearchPlaceholder.Text = "Поиск по ФИО врача...";
            }
            else
            {
                SearchPlaceholder.Text = "Поиск по ФИО пациента...";
            }
        }
        private void ApplySearchFilter()
        {
            string searchTerm = SearchBox?.Text?.Trim();
            bool hasSearch = !string.IsNullOrEmpty(searchTerm);
            SearchPlaceholder.Visibility = hasSearch ? Visibility.Collapsed : Visibility.Visible;
            if (!hasSearch)
            {
                TreatmentsGrid.ItemsSource = _allTreatments;
            }
            else
            {
                string[] searchWords = searchTerm.ToLowerInvariant().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var filtered = _allTreatments.Where(t =>
                {
                    string target = _patientIdFilter.HasValue ? t.DoctorName : t.PatientFullName;
                    string targetLower = target.ToLowerInvariant();
                    return searchWords.All(word => targetLower.Contains(word));
                }).ToList();

                TreatmentsGrid.ItemsSource = filtered;
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
                NavigationService?.Navigate(new DoctorViews.TreatmentDetailView(prescriptionId));
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && NavigationService?.CanGoBack == true)
            {
                GoBack();
                e.Handled = true;
            }
        }
        private void GoBack()
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Нет страниц для возврата.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}