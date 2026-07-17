using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class DoctorLabResultDetailView : Page
    {
        private readonly int _labOrderId;
        public DoctorLabResultDetailView(int labOrderId)
        {
            InitializeComponent();
            QuestPDF.Settings.License = LicenseType.Community;
            _labOrderId = labOrderId;
            LoadResults(labOrderId);
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
        }
        private void LoadResults(int labOrderId)
        {
            using var context = new ClinicSoftContext();
            var rawResults = context.LabResultItems
                .Include(ri => ri.TestType)
                    .ThenInclude(tt => tt.Unit)
                .Where(ri => ri.LabResult.LabOrderId == labOrderId)
                .ToList();
            var results = rawResults.Select(ri => new
            {
                TestName = ri.TestType.Name,
                ResultValue = ri.ResultValue,
                UnitSymbol = ri.TestType.Unit?.Symbol ?? "—",
                NormalRange = ri.TestType.NormalRange ?? "—"
            }).ToList();
            ResultsGrid.ItemsSource = results;
        }
        private void BtnSavePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = (System.Collections.IList)ResultsGrid.ItemsSource;
                if (items == null || items.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var document = GeneratePdfDocument(items);
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Анализы_{_labOrderId}_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                    DefaultExt = ".pdf"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    document.GeneratePdf(saveDialog.FileName);
                    MessageBox.Show($"Результаты успешно сохранены:\n{saveDialog.FileName}",
                                  "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании PDF:\n{ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private Document GeneratePdfDocument(System.Collections.IList items)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Text("Медицинская клиника «ClinicSoft»")
                                 .FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();   
                            columns.ConstantColumn(80); 
                            columns.ConstantColumn(60); 
                            columns.ConstantColumn(100);
                        });
                        table.Cell().Element(HeaderStyle);
                        table.Cell().Element(HeaderStyle);
                        table.Cell().Element(HeaderStyle);
                        table.Cell().Element(HeaderStyle);
                        foreach (var item in items)
                        {
                            var testName = item.GetType().GetProperty("TestName")?.GetValue(item)?.ToString() ?? "";
                            var result = item.GetType().GetProperty("ResultValue")?.GetValue(item)?.ToString() ?? "";
                            var unit = item.GetType().GetProperty("UnitSymbol")?.GetValue(item)?.ToString() ?? "";
                            var range = item.GetType().GetProperty("NormalRange")?.GetValue(item)?.ToString() ?? "";
                            table.Cell().Border(1).Padding(5).Text(testName);
                            table.Cell().Border(1).Padding(5).Text(result);
                            table.Cell().Border(1).Padding(5).Text(unit);
                            table.Cell().Border(1).Padding(5).Text(range);
                        }
                    });
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9));
                        text.Span("Результаты лабораторных исследований | ");
                        text.Span($"ID заказа: {_labOrderId} | ");
                        text.Span($"{DateTime.Now:dd.MM.yyyy HH:mm}");
                    });
                });
            });
        }
        static void HeaderStyle(IContainer container)
        {
            container
                .Background(Colors.Grey.Lighten3)
                .PaddingVertical(5)
                .PaddingHorizontal(5);
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