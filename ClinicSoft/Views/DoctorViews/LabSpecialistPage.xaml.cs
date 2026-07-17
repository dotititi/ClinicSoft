using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class LabSpecialistPage : Page
    {
        private readonly int _doctorId;
        private readonly bool _isLabSpecialist;
        public bool IsLabSpecialist => _isLabSpecialist;
        private dynamic[] _allPendingOrders = Array.Empty<dynamic>();
        private dynamic[] _allCompletedOrders = Array.Empty<dynamic>();
        public LabSpecialistPage(int doctorId, bool isLabSpecialist = false)
        {
            InitializeComponent();
            _doctorId = doctorId;
            _isLabSpecialist = isLabSpecialist;
            SetupPendingGridColumns();
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                LoadPendingOrders(_isLabSpecialist);
                LoadCompletedOrders(_isLabSpecialist);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadPendingOrders(bool forAllDoctors)
        {
            using var context = new ClinicSoftContext();
            var query = context.LabOrders
                .Include(lo => lo.Patient)
                .Include(lo => lo.LabOrderItems)
                    .ThenInclude(item => item.TestType)
                .Where(lo => lo.Status == "pending" || lo.Status == "in_progress");
            if (!forAllDoctors)
            {
                query = query.Where(lo => lo.DoctorId == _doctorId);
            }
            _allPendingOrders = query
                .OrderByDescending(lo => lo.OrderedAt)
                .Select(lo => new
                {
                    Id = lo.Id,
                    OrderedAt = lo.OrderedAt,
                    PatientName = lo.Patient != null
                        ? $"{lo.Patient.LastName} {lo.Patient.FirstName} {lo.Patient.MiddleName}"
                        : "[Пациент не указан]",
                    PatientFullName = lo.Patient != null
                        ? $"{lo.Patient.LastName} {lo.Patient.FirstName} {lo.Patient.MiddleName}".Trim()
                        : "[Пациент не указан]",
                    TestTypes = lo.LabOrderItems != null && lo.LabOrderItems.Any()
                        ? string.Join("\n", lo.LabOrderItems
                            .Where(item => item.TestType != null)
                            .Select(item => item.TestType.Name))
                        : "[Нет анализов]"
                })
                .ToArray();
            ApplySearchFilter();
        }
        private void SetupPendingGridColumns()
        {
            var actionColumns = PendingOrdersGrid.Columns
                .OfType<DataGridTemplateColumn>()
                .Where(c => c.Header?.ToString() == "Действия")
                .ToList();
            foreach (var column in actionColumns)
            {
                PendingOrdersGrid.Columns.Remove(column);
            }
            if (_isLabSpecialist)
            {
                var actionsColumn = new DataGridTemplateColumn
                {
                    Header = "Действия",
                    Width = new DataGridLength(145),
                    MinWidth = 140,
                    CellTemplate = CreateFillResultsButtonTemplate()
                };
                PendingOrdersGrid.Columns.Add(actionsColumn);
            }
        }
        private DataTemplate CreateFillResultsButtonTemplate()
        {
            var factory = new FrameworkElementFactory(typeof(Button));
            factory.SetValue(Button.ContentProperty, "Заполнить результаты");
            factory.SetValue(Button.TagProperty, new Binding("Id"));
            factory.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnFillResults_Click));
            factory.SetValue(Button.PaddingProperty, new Thickness(8, 2, 8, 2));
            factory.SetValue(Button.BackgroundProperty, Brushes.Transparent);
            factory.SetValue(Button.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2")));
            factory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
            factory.SetValue(Button.CursorProperty, Cursors.Hand);

            return new DataTemplate { VisualTree = factory };
        }
        private void LoadCompletedOrders(bool forAllDoctors)
        {
            using var context = new ClinicSoftContext();
            var query = context.LabOrders
                .Include(lo => lo.Patient)
                .Include(lo => lo.LabOrderItems)
                    .ThenInclude(item => item.TestType)
                .Where(lo => lo.Status == "completed");
            if (!forAllDoctors)
            {
                query = query.Where(lo => lo.DoctorId == _doctorId);
            }
            _allCompletedOrders = query
                .OrderByDescending(lo => lo.OrderedAt)
                .Select(lo => new
                {
                    Id = lo.Id,
                    OrderedAt = lo.OrderedAt,
                    PatientName = lo.Patient != null
                        ? $"{lo.Patient.LastName} {lo.Patient.FirstName}"
                        : "[Пациент не указан]",
                    PatientFullName = lo.Patient != null
                        ? $"{lo.Patient.LastName} {lo.Patient.FirstName} {lo.Patient.MiddleName}".Trim()
                        : "[Пациент не указан]",
                    TestTypes = lo.LabOrderItems != null && lo.LabOrderItems.Any()
                        ? string.Join("\n", lo.LabOrderItems
                            .Where(item => item.TestType != null)
                            .Select(item => item.TestType.Name))
                        : "[Нет анализов]"
                })
                .ToArray();
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
                    .Where(o => !string.IsNullOrEmpty(o.PatientFullName) &&
                               o.PatientFullName.ToString().ToLowerInvariant().Contains(searchLower))
                    .ToArray();

                var filteredCompleted = _allCompletedOrders
                    .Where(o => !string.IsNullOrEmpty(o.PatientFullName) &&
                               o.PatientFullName.ToString().ToLowerInvariant().Contains(searchLower))
                    .ToArray();

                PendingOrdersGrid.ItemsSource = filteredPending;
                CompletedOrdersGrid.ItemsSource = filteredCompleted;
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }
        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int orderId)
            {
                if (_isLabSpecialist)
                {
                    var resultWindow = new LabResultWindow(orderId, _doctorId);
                    if (resultWindow.ShowDialog() == true)
                    {
                        LoadData();
                    }
                }
                else
                {
                    NavigationService?.Navigate(new DoctorLabResultDetailView(orderId));
                }
            }
        }
        private void BtnFillResults_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int orderId)
            {
                var resultWindow = new LabResultWindow(orderId, _doctorId);
                if (resultWindow.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService?.Navigate(new DoctorDashboardPage(_doctorId));
            }
        }
    }
}