using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace ClinicSoft.Views.Registrator
{
    public partial class DoctorsListPage : Page
    {
        private List<DoctorDisplayModel> _allDoctors = new();

        public DoctorsListPage()
        {
            InitializeComponent();
            LoadDoctors();
            UpdateSearchPlaceholder();
        }
        private void LoadDoctors()
        {
            try
            {
                using var context = new ClinicSoftContext();
                _allDoctors = context.Doctors
                    .Include(d => d.Speciality)
                    .Include(d => d.Office)
                    .Include(d => d.Status)
                    .AsEnumerable()
                    .Select(d => new DoctorDisplayModel
                    {
                        FullName = $"{d.LastName} {d.FirstName} {(string.IsNullOrEmpty(d.MiddleName) ? "" : d.MiddleName)}".Trim(),
                        Speciality = d.Speciality?.Name ?? "Не указана",
                        Office = d.Office?.Number ?? "Не назначен",
                        Status = d.Status?.Name ?? "Неизвестен"
                    })
                    .OrderBy(d => d.FullName)
                    .ToList();

                ApplyFilter();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка загрузки списка врачей:\n{ex.Message}",
                    "Ошибка",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                DgDoctors.ItemsSource = new List<DoctorDisplayModel>();
            }
        }
        private void ApplyFilter()
        {
            string searchTerm = TxtSearch.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(searchTerm))
            {
                DgDoctors.ItemsSource = _allDoctors;
            }
            else
            {
                var filtered = _allDoctors
                    .Where(d => d.FullName.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase) ||
                               d.Office.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();
                DgDoctors.ItemsSource = filtered;
            }
            UpdateSearchPlaceholder();
        }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }
        private void UpdateSearchPlaceholder()
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(TxtSearch.Text)
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Hidden;
        }
        private class DoctorDisplayModel
        {
            public string FullName { get; set; } = string.Empty;
            public string Speciality { get; set; } = string.Empty;
            public string Office { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }
    }
}