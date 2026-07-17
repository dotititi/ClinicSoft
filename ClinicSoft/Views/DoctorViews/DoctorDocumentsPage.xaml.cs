using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using Microsoft.Win32;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class DoctorDocumentsPage : Page
    {
        private readonly int _doctorId;
        private string _currentSearch = "";
        public DoctorDocumentsPage(int doctorId)
        {
            InitializeComponent();
            _doctorId = doctorId;
            QuestPDF.Settings.License = LicenseType.Community;
            LoadGeneratedDocuments();
        }
        private void LoadGeneratedDocuments(string searchQuery = "")
        {
            using var context = new ClinicSoftContext();
            var baseQuery = context.Documents
                .Include(d => d.Patient)
                .Include(d => d.DocumentTemplate)
                    .ThenInclude(t => t.DocumentType)
                .Where(d => d.DoctorId == _doctorId);
            var allDocuments = baseQuery.ToList();
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                string termLower = searchQuery.Trim().ToLowerInvariant();
                allDocuments = allDocuments
                    .Where(d => d.Patient != null &&
                               (
                                   (d.Patient.LastName?.ToLowerInvariant().Contains(termLower) == true) ||
                                   (d.Patient.FirstName?.ToLowerInvariant().Contains(termLower) == true) ||
                                   (d.Patient.MiddleName?.ToLowerInvariant().Contains(termLower) == true)
                               ))
                    .ToList();
            }
            var documents = allDocuments
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new
                {
                    Id = d.Id,
                    CreatedAt = d.CreatedAt,
                    PatientName = $"{d.Patient.LastName} {d.Patient.FirstName} {d.Patient.MiddleName}".Trim(),
                    DocumentTypeName = d.DocumentTemplate?.DocumentType?.Name ?? "Неизвестный тип"
                })
                .ToList();
            GeneratedDocumentsGrid.ItemsSource = documents;
        }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSearchPlaceholder();
            _currentSearch = TxtSearch.Text;
            LoadGeneratedDocuments(_currentSearch);
        }
        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateSearchPlaceholder();
        }
        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateSearchPlaceholder();
        }
        private void UpdateSearchPlaceholder()
        {
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(TxtSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        private void BtnCreateDocument_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new CreateDocumentWindow(_doctorId);
            if (createWindow.ShowDialog() == true)
            {
                LoadGeneratedDocuments(_currentSearch);
            }
        }
        private void BtnSavePdf_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int documentId)
                return;
            try
            {
                using var context = new ClinicSoftContext();
                var document = context.Documents
                    .Include(d => d.Patient)
                    .Include(d => d.Doctor)
                        .ThenInclude(doc => doc.Speciality)
                    .Include(d => d.DocumentTemplate)
                        .ThenInclude(t => t.DocumentType)
                    .FirstOrDefault(d => d.Id == documentId);
                if (document == null)
                {
                    MessageBox.Show("Документ не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var documentContent = GeneratePdfContent(document);
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Документ_{document.Id}_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                    DefaultExt = ".pdf"
                };
                if (saveDialog.ShowDialog() == true)
                {
                    documentContent.GeneratePdf(saveDialog.FileName);
                    MessageBox.Show($"Документ успешно сохранён:\n{saveDialog.FileName}",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании PDF:\n{ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }
        private Document GeneratePdfContent(ClinicSoft.Models.Document doc)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));
                    page.Header().Text("Медицинская клиника «ClinicSoft»")
                                 .FontSize(16).Bold().FontColor(Colors.Blue.Medium);
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
                        column.Item().PaddingTop(5).Text("Тип документа:").Bold();
                        column.Item().Text(doc.DocumentTemplate?.DocumentType?.Name ?? "Не указан");
                        column.Item().PaddingTop(15).Text("Содержание документа:").Bold();
                        column.Item().Text(doc.Content ?? "Нет данных");
                    });
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Документ сформирован автоматически. ");
                        text.Span($"ID: {doc.Id} | Подпись врача: ___________________");
                    });
                });
            });
        }
        private void BtnViewDocument_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int documentId)
            {
                var viewWindow = new DocumentViewWindow(documentId, isEditable: false);
                viewWindow.ShowDialog();
            }
        }
        private void BtnEditDocument_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int documentId)
            {
                var editWindow = new DocumentViewWindow(documentId, isEditable: true);
                if (editWindow.ShowDialog() == true)
                {
                    LoadGeneratedDocuments(_currentSearch);
                }
            }
        }
    }
}