using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Registrator
{
    public partial class LabOrderDetailView : Page
    {
        private readonly int _labOrderId;
        private class LabResultItemDto
        {
            public string TestName { get; set; } = string.Empty;
            public string ResultValue { get; set; } = string.Empty;
            public string Unit { get; set; } = string.Empty;
            public string ReferenceRange { get; set; } = string.Empty;
        }
        public LabOrderDetailView(int labOrderId)
        {
            InitializeComponent();
            QuestPDF.Settings.License = LicenseType.Community;
            _labOrderId = labOrderId;
            LoadLabOrderDetails(labOrderId);
        }
        private void LoadLabOrderDetails(int labOrderId)
        {
            using var context = new ClinicSoftContext();
            var labOrder = context.LabOrders
                .Include(lo => lo.Patient)
                .Include(lo => lo.Doctor)
                .Include(lo => lo.LabOrderItems)
                    .ThenInclude(item => item.TestType)
                        .ThenInclude(tt => tt.Unit)
                .Include(lo => lo.LabResult)
                    .ThenInclude(lr => lr.LabResultItems)
                        .ThenInclude(lri => lri.TestType)
                            .ThenInclude(tt => tt.Unit)
                .FirstOrDefault(lo => lo.Id == labOrderId);

            if (labOrder == null) return;
            PatientNameText.Text = $"{labOrder.Patient.LastName} {labOrder.Patient.FirstName} {labOrder.Patient.MiddleName}".Trim();
            OrderDateText.Text = labOrder.OrderedAt.ToString("dd.MM.yyyy HH:mm");
            DoctorNameText.Text = $"{labOrder.Doctor.LastName} {labOrder.Doctor.FirstName}";
            StatusText.Text = TranslateStatus(labOrder.Status);
            var assignedTests = labOrder.LabOrderItems.Select(item => new
            {
                TestName = item.TestType.Name,
                Unit = item.TestType.Unit?.Symbol ?? "—",
                NormalRange = item.TestType.NormalRange ?? "—"
            }).ToList();
            AssignedTestsGrid.ItemsSource = assignedTests;
            var results = labOrder.LabResult?.LabResultItems?
                .Select(item => new LabResultItemDto
                {
                    TestName = item.TestType.Name,
                    ResultValue = item.ResultValue,
                    Unit = item.TestType?.Unit?.Symbol ?? "—",
                    ReferenceRange = item.TestType?.NormalRange ?? "—"
                })
                .ToList() ?? new List<LabResultItemDto>();

            ResultsGrid.ItemsSource = results;

            ResultsGrid.ItemsSource = results;
        }
        private string TranslateStatus(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "pending" => "Ожидает",
                "in_progress" => "В работе",
                "completed" => "Завершён",
                _ => status ?? "Неизвестен"
            };
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
            else
            {
                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    parentWindow.Close();
                }
            }
        }
        private void BtnSavePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var context = new ClinicSoftContext();
                var labOrder = context.LabOrders
                    .Include(lo => lo.Patient)
                    .Include(lo => lo.Doctor)
                    .Include(lo => lo.LabOrderItems)
                        .ThenInclude(item => item.TestType)
                            .ThenInclude(tt => tt.Unit)
                    .Include(lo => lo.LabResult)
                        .ThenInclude(lr => lr.LabResultItems)
                            .ThenInclude(lri => lri.TestType)
                                .ThenInclude(tt => tt.Unit)
                    .FirstOrDefault(lo => lo.Id == _labOrderId);
                if (labOrder == null)
                {
                    MessageBox.Show("Заказ не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var document = GeneratePdfDocument(labOrder);
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Анализ_{_labOrderId}_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                    DefaultExt = ".pdf"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    document.GeneratePdf(saveDialog.FileName);
                    MessageBox.Show($"Документ успешно сохранён:\n{saveDialog.FileName}",
                                  "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании PDF:\n{ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private Document GeneratePdfDocument(ClinicSoft.Models.LabOrder order)
        {
            var assignedTests = order.LabOrderItems.Select(item => new
            {
                Name = item.TestType.Name,
                Unit = item.TestType.Unit?.Symbol ?? "—",
                Range = item.TestType.NormalRange ?? "—"
            }).ToList();
            var results = order.LabResult?.LabResultItems?
                    .Select(item => new LabResultItemDto
                    {
                        TestName = item.TestType.Name,
                        ResultValue = item.ResultValue,
                        Unit = item.TestType?.Unit?.Symbol ?? "—",
                        ReferenceRange = item.TestType?.NormalRange ?? "—"
                    })
                    .ToList() ?? new List<LabResultItemDto>();
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Text("Медицинская клиника «ClinicSoft»")
                                 .FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                    page.Content().Column(column =>
                    {
                        column.Item().Text("Общая информация").Bold().FontSize(12).Underline();
                        column.Item().PaddingTop(5).Text($"Пациент: {order.Patient.LastName} {order.Patient.FirstName} {order.Patient.MiddleName}".Trim());
                        column.Item().Text($"Дата: {order.OrderedAt:dd.MM.yyyy HH:mm}");
                        column.Item().Text($"Врач: {order.Doctor.LastName} {order.Doctor.FirstName}");
                        column.Item().Text($"Статус: {TranslateStatus(order.Status)}");
                        column.Item().PaddingTop(10);
                        if (assignedTests.Any())
                        {
                            column.Item().Text("Назначенные анализы").Bold().FontSize(12).Underline();
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c => { c.RelativeColumn(); c.ConstantColumn(60); c.ConstantColumn(100); });
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Анализ").Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Ед.").Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Норма").Bold();
                                foreach (var test in assignedTests)
                                {
                                    table.Cell().Border(1).Padding(5).Text(test.Name);
                                    table.Cell().Border(1).Padding(5).Text(test.Unit);
                                    table.Cell().Border(1).Padding(5).Text(test.Range);
                                }
                            });
                            column.Item().PaddingTop(10);
                        }
                        if (results.Any())
                        {
                            column.Item().Text("Результаты").Bold().FontSize(12).Underline();
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c => { c.RelativeColumn(); c.ConstantColumn(80); c.ConstantColumn(60); c.ConstantColumn(100); });
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Анализ").Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Результат").Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Ед.").Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Норма").Bold();
                                foreach (var res in results)
                                {
                                    table.Cell().Border(1).Padding(5).Text(res.TestName);
                                    table.Cell().Border(1).Padding(5).Text(res.ResultValue);
                                    table.Cell().Border(1).Padding(5).Text(res.Unit);
                                    table.Cell().Border(1).Padding(5).Text(res.ReferenceRange);
                                }
                            });
                        }
                    });
                    page.Footer().AlignCenter()
                               .Text($"Лабораторный заказ | ID: {_labOrderId} | {DateTime.Now:dd.MM.yyyy HH:mm}")
                               .FontSize(9);
                });
            });
        }
    }
}