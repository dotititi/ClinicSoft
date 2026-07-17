using ClinicSoft.Data;
using ClinicSoft.Views.DoctorViews;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Registrator
{
    public partial class RegistratorDocumentsPage : Page
    {
        private readonly int _patientId;
        private string _currentSearch = "";
        public RegistratorDocumentsPage(int patientId)
        {
            InitializeComponent();
            QuestPDF.Settings.License = LicenseType.Community;
            _patientId = patientId;
            LoadDocuments();
        }
        private void LoadDocuments(string searchQuery = "")
        {
            try
            {
                using var context = new ClinicSoftContext();
                var documents = context.Documents
                    .Where(d => d.PatientId == _patientId)
                    .Include(d => d.DocumentTemplate)
                        .ThenInclude(t => t.DocumentType)
                    .AsEnumerable()
                    .Select(d => new
                    {
                        Id = d.Id,
                        CreatedAt = d.CreatedAt,
                        DocumentTypeName = d.DocumentTemplate?.DocumentType?.Name ?? "Без типа"
                    })
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    var term = searchQuery.Trim().ToLowerInvariant();
                    documents = documents
                        .Where(d => d.DocumentTypeName.ToLowerInvariant().Contains(term))
                        .ToList();
                }
                DocumentsGrid.ItemsSource = documents;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки документов:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearch = TxtSearch.Text;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearch)
                ? Visibility.Visible : Visibility.Collapsed;
            LoadDocuments(_currentSearch);
        }
        private void BtnOpenDocument_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int documentId)
            {
                var viewWindow = new DocumentViewWindow(documentId, isEditable: false);
                viewWindow.ShowDialog();
            }
        }
        private void BtnSavePdf_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int documentId)
                return;
            try
            {
                using var context = new ClinicSoftContext();
                var doc = context.Documents
                    .Include(d => d.Patient)
                    .Include(d => d.Doctor)
                        .ThenInclude(doc => doc.Speciality)
                    .Include(d => d.DocumentTemplate)
                        .ThenInclude(t => t.DocumentType)
                    .FirstOrDefault(d => d.Id == documentId);
                if (doc == null)
                {
                    MessageBox.Show("Документ не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var pdfDoc = GeneratePdfDocument(doc);
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Документ_{documentId}_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                    DefaultExt = ".pdf"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    pdfDoc.GeneratePdf(saveDialog.FileName);
                    MessageBox.Show("PDF успешно сохранён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании PDF:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private Document GeneratePdfDocument(ClinicSoft.Models.Document doc)
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
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Item().Text(doc.DocumentTemplate?.Name ?? "Медицинский документ")
                                    .FontSize(14).Bold().Underline();
                        column.Item().PaddingTop(10).Text($"Дата выдачи: {doc.CreatedAt:dd.MM.yyyy HH:mm}");
                        column.Item().PaddingTop(10).Text("Пациент:").Bold();
                        column.Item().Text($"{doc.Patient?.LastName} {doc.Patient?.FirstName} {doc.Patient?.MiddleName}".Trim());
                        column.Item().PaddingTop(5).Text("Врач:").Bold();
                        column.Item().Text($"{doc.Doctor?.LastName} {doc.Doctor?.FirstName} {doc.Doctor?.MiddleName}".Trim());
                        column.Item().Text($"({doc.Doctor?.Speciality?.Name ?? "без специальности"})");
                        column.Item().PaddingTop(10).Text("Тип документа:").Bold();
                        column.Item().Text(doc.DocumentTemplate?.DocumentType?.Name ?? "Не указан");

                        column.Item().PaddingTop(15).Text("Содержание документа:").Bold();
                        column.Item().Text(doc.Content ?? "Нет данных");
                    });
                    page.Footer().AlignCenter()
                               .Text($"ID документа: {doc.Id} | Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                               .FontSize(9);
                });
            });
        }
    }
}