using ClinicSoft.Data;
using ClinicSoft.Views.DoctorViews;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSoft.Views.Registrator
{
    public partial class RegistratorLabTestsPage : Page
    {
        private readonly int _patientId;
        private List<dynamic> _allPendingOrders = new();
        private List<dynamic> _allCompletedOrders = new();
        public RegistratorLabTestsPage(int patientId)
        {
            InitializeComponent();
            _patientId = patientId;
            LoadPatientInfo();
            LoadLabOrders();
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
        }
        private void LoadPatientInfo()
        {
            using var context = new ClinicSoftContext();
            var patient = context.Patients.FirstOrDefault(p => p.Id == _patientId);
            if (patient != null)
            {
                PatientHeader.Text = $"Анализы: {patient.LastName} {patient.FirstName} {patient.MiddleName}".Trim();
            }
        }
        private void LoadLabOrders()
        {
            LoadPendingOrders();
            LoadCompletedOrders();
        }
        private static string TranslateStatus(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "pending" => "Ожидает",
                "in_progress" => "В работе",
                "completed" => "Завершён",
                _ => status ?? "Неизвестен"
            };
        }
        private void LoadPendingOrders()
        {
            using var context = new ClinicSoftContext();
            _allPendingOrders = context.LabOrders
                .Include(lo => lo.Doctor)
                .Include(lo => lo.LabOrderItems)
                    .ThenInclude(item => item.TestType)
                        .ThenInclude(tt => tt.Unit)
                .Where(lo => lo.PatientId == _patientId &&
                            (lo.Status == "pending" || lo.Status == "in_progress"))
                .ToList()
                .Select(lo => new
                {
                    OrderedAt = lo.OrderedAt,
                    DoctorName = $"{lo.Doctor.LastName} {lo.Doctor.FirstName} {lo.Doctor.MiddleName}",
                    TestTypes = string.Join("\n", lo.LabOrderItems.Select(item =>
                    {
                        var testName = item.TestType?.Name ?? "—";
                        var unitSymbol = item.TestType?.Unit?.Symbol ?? "—";
                        var normalRange = item.TestType?.NormalRange ?? "—";
                        return $"{testName} [{unitSymbol}] (Норма: {normalRange})";
                    })),
                    Status = TranslateStatus(lo.Status)
                })
                .Cast<dynamic>()
                .ToList();

            ApplySearchFilter();
        }
        private void LoadCompletedOrders()
        {
            using var context = new ClinicSoftContext();
            _allCompletedOrders = context.LabOrders
                .Include(lo => lo.Doctor)
                .Include(lo => lo.LabOrderItems)
                    .ThenInclude(item => item.TestType)
                        .ThenInclude(tt => tt.Unit)
                .Where(lo => lo.PatientId == _patientId && lo.Status == "completed")
                .ToList()
                .Select(lo => new
                {
                    Id = lo.Id,
                    OrderedAt = lo.OrderedAt,
                    DoctorName = $"{lo.Doctor.LastName} {lo.Doctor.FirstName} {lo.Doctor.MiddleName}",
                    TestTypes = string.Join("\n", lo.LabOrderItems.Select(item =>
                    {
                        var testName = item.TestType?.Name ?? "—";
                        var unitSymbol = item.TestType?.Unit?.Symbol ?? "—";
                        var normalRange = item.TestType?.NormalRange ?? "—";
                        return $"{testName} [{unitSymbol}] (Норма: {normalRange})";
                    })),
                    Status = TranslateStatus(lo.Status)
                })
                .Cast<dynamic>()
                .ToList();

            ApplySearchFilter();
        }
        private void ApplySearchFilter()
        {
            string searchTerm = SearchBox?.Text?.Trim();
            bool hasSearch = !string.IsNullOrEmpty(searchTerm);
            SearchPlaceholder.Visibility = hasSearch ? Visibility.Collapsed : Visibility.Visible;
            if (!hasSearch)
            {
                PendingOrdersGrid.ItemsSource = _allPendingOrders;
                CompletedOrdersGrid.ItemsSource = _allCompletedOrders;
            }
            else
            {
                string searchLower = searchTerm.ToLowerInvariant();
                var filteredPending = _allPendingOrders
                    .Where(o => ContainsSearchTerm(o.DoctorName, o.TestTypes, searchLower))
                    .ToList();
                var filteredCompleted = _allCompletedOrders
                    .Where(o => ContainsSearchTerm(o.DoctorName, o.TestTypes, searchLower))
                    .ToList();
                PendingOrdersGrid.ItemsSource = filteredPending;
                CompletedOrdersGrid.ItemsSource = filteredCompleted;
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }
        private bool ContainsSearchTerm(string doctorName, string testTypes, string searchLower)
        {
            return (doctorName?.ToLowerInvariant().Contains(searchLower) == true) ||
                   (testTypes?.ToLowerInvariant().Contains(searchLower) == true);
        }
        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int labOrderId)
            {
                NavigationService?.Navigate(new LabOrderDetailView(labOrderId));
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