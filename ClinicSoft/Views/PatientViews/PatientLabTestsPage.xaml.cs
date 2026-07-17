using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.PatientViews
{
    public partial class PatientLabTestsPage : Page
    {
        private readonly int _patientId;
        private System.Collections.Generic.List<dynamic> _allCompletedOrders = new();
        private System.Collections.Generic.List<dynamic> _allPendingOrders = new();
        public PatientLabTestsPage(int patientUserId)
        {
            InitializeComponent();
            _patientId = GetPatientIdByUserId(patientUserId);
            LoadLabOrders();
        }
        private int GetPatientIdByUserId(int userId)
        {
            using var context = new ClinicSoftContext();
            return context.Patients
                .Where(p => p.UserId == userId)
                .Select(p => p.Id)
                .FirstOrDefault();
        }
        private void LoadLabOrders()
        {
            LoadCompletedOrders();
            LoadPendingOrders();
        }
        private void LoadCompletedOrders()
        {
            using var context = new ClinicSoftContext();
            _allCompletedOrders = context.LabResults
                .Include(lr => lr.LabOrder)
                    .ThenInclude(lo => lo.LabOrderItems)
                        .ThenInclude(item => item.TestType)
                .Where(lr => lr.LabOrder.PatientId == _patientId)
                .OrderByDescending(lr => lr.CompletedAt)
                .Select(lr => new
                {
                    LabOrderId = lr.LabOrder.Id,
                    CompletedAt = lr.CompletedAt,
                    TestTypes = string.Join("\n", lr.LabOrder.LabOrderItems.Select(i => i.TestType.Name))
                })
                .Cast<dynamic>()
                .ToList();
            ApplySearchFilter();
        }
        private void LoadPendingOrders()
        {
            using var context = new ClinicSoftContext();
            _allPendingOrders = context.LabOrders
                .Include(lo => lo.Doctor)
                .Include(lo => lo.LabOrderItems)
                    .ThenInclude(item => item.TestType)
                .Where(lo => lo.PatientId == _patientId && lo.Status != "completed")
                .OrderByDescending(lo => lo.OrderedAt)
                .Select(lo => new
                {
                    OrderedAt = lo.OrderedAt,
                    DoctorName = $"{lo.Doctor.LastName} {lo.Doctor.FirstName} {lo.Doctor.MiddleName}",
                    TestTypes = string.Join("\n", lo.LabOrderItems.Select(i => i.TestType.Name)),
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
                CompletedOrdersGrid.ItemsSource = _allCompletedOrders;
                PendingOrdersGrid.ItemsSource = _allPendingOrders;
            }
            else
            {
                string searchLower = searchTerm.ToLowerInvariant();

                var filteredCompleted = _allCompletedOrders
                    .Where(o => o.TestTypes?.ToString().ToLowerInvariant().Contains(searchLower) == true)
                    .ToList();

                var filteredPending = _allPendingOrders
                    .Where(o => o.TestTypes?.ToString().ToLowerInvariant().Contains(searchLower) == true)
                    .ToList();

                CompletedOrdersGrid.ItemsSource = filteredCompleted;
                PendingOrdersGrid.ItemsSource = filteredPending;
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }
        private static string TranslateStatus(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "pending" => "Ожидает",
                "in_progress" => "В работе",
                _ => status ?? "Неизвестен"
            };
        }
        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int labOrderId)
            {
                NavigationService?.Navigate(new PatientLabResultDetailView(labOrderId));
            }
        }
    }
}