using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class LabResultWindow : Window
    {
        private readonly int _orderId;
        private readonly int _doctorId;
        private List<LabResultViewModel> _testViewModels = new();
        public LabResultWindow(int orderId, int doctorId)
        {
            InitializeComponent();
            _orderId = orderId;
            _doctorId = doctorId;
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var order = context.LabOrders
                    .Include(o => o.Patient)
                        .ThenInclude(p => p.GenderCodeNavigation)
                    .Include(o => o.LabOrderItems)
                        .ThenInclude(item => item.TestType)
                            .ThenInclude(tt => tt.Unit)
                    .FirstOrDefault(o => o.Id == _orderId);
                if (order == null)
                {
                    MessageBox.Show("Заказ не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }
                var existingResults = context.LabResults
                    .Include(lr => lr.LabResultItems)
                    .Where(r => r.LabOrderId == _orderId)
                    .SelectMany(lr => lr.LabResultItems)
                    .ToDictionary(ri => ri.TestTypeId, ri => ri.ResultValue);
                PatientInfo.Text = $"Пациент: {order.Patient?.LastName} {order.Patient?.FirstName} {order.Patient?.MiddleName}".Trim();
                _testViewModels = order.LabOrderItems
                    .OrderBy(item => item.TestType.Name)
                    .Select(item => new LabResultViewModel
                    {
                        OrderItemId = item.Id,
                        TestTypeId = item.TestTypeId,
                        TestName = item.TestType.Name,
                        ResultValue = existingResults.GetValueOrDefault(item.TestTypeId),
                        UnitSymbol = item.TestType.Unit?.Symbol ?? "—",
                        NormalRange = item.TestType.NormalRange ?? "—"
                    })
                    .ToList();
                ResultsItemsControl.ItemsSource = _testViewModels;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                Close();
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var context = new ClinicSoftContext();
                var labOrder = context.LabOrders.FirstOrDefault(lo => lo.Id == _orderId);
                if (labOrder != null)
                {
                    labOrder.Status = "completed";
                }
                var labResult = context.LabResults
                    .Include(lr => lr.LabResultItems)
                    .FirstOrDefault(lr => lr.LabOrderId == _orderId);
                if (labResult == null)
                {
                    labResult = new LabResult
                    {
                        LabOrderId = _orderId,
                        CompletedAt = DateTime.Now,
                        PerformedBy = _doctorId,
                        LabResultItems = new List<LabResultItem>()
                    };
                    context.LabResults.Add(labResult);
                }
                else
                {
                    labResult.CompletedAt = DateTime.Now;
                    labResult.PerformedBy = _doctorId;
                }
                var existingItemsDict = labResult.LabResultItems
                    .ToDictionary(item => item.TestTypeId, item => item);
                foreach (var viewModel in _testViewModels)
                {
                    if (!string.IsNullOrWhiteSpace(viewModel.ResultValue))
                    {
                        if (existingItemsDict.TryGetValue(viewModel.TestTypeId, out var existingItem))
                        {
                            existingItem.ResultValue = viewModel.ResultValue.Trim();
                        }
                        else
                        {
                            var newItem = new LabResultItem
                            {
                                TestTypeId = viewModel.TestTypeId,
                                ResultValue = viewModel.ResultValue.Trim()
                            };
                            labResult.LabResultItems.Add(newItem);
                        }
                    }
                    else
                    {
                        if (existingItemsDict.TryGetValue(viewModel.TestTypeId, out var itemToRemove))
                        {
                            context.LabResultItems.Remove(itemToRemove);
                        }
                    }
                }
                context.SaveChanges();
                MessageBox.Show("Результаты успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
    public class LabResultViewModel
    {
        public int OrderItemId { get; set; }
        public int TestTypeId { get; set; }
        public string TestName { get; set; } = null!;
        public string? ResultValue { get; set; }
        public string UnitSymbol { get; set; } = "—";
        public string NormalRange { get; set; } = "—";
    }
}