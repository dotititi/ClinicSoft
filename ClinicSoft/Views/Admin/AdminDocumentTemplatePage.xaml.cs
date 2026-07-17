using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminDocumentTemplatePage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 580;
        private DocumentTemplate _selectedTemplate;
        private string _currentSearchText = "";
        public AdminDocumentTemplatePage()
        {
            InitializeComponent();
            LoadTemplates();
        }
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth < MIN_WIDTH_FOR_SINGLE_LINE)
            {
                SingleLineToolbar.Visibility = Visibility.Collapsed;
                MultiLineToolbar.Visibility = Visibility.Visible;
            }
            else
            {
                SingleLineToolbar.Visibility = Visibility.Visible;
                MultiLineToolbar.Visibility = Visibility.Collapsed;
            }
            UpdateSearchDisplay();
        }
        private void LoadTemplates(string searchTerm = "")
        {
            using var context = new ClinicSoftContext();
            var allTemplates = context.DocumentTemplates
                .Include(t => t.DocumentType)
                .OrderBy(t => t.Name)
                .ToList();
            foreach (var t in allTemplates)
            {
                if (t.DocumentType == null)
                {
                    t.DocumentType = new DocumentType { Name = "(тип удалён)" };
                }
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.Trim().ToLowerInvariant();
                allTemplates = allTemplates
                    .Where(t => t.Name != null &&
                               t.Name.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            TemplateDataGrid.ItemsSource = allTemplates;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadTemplates(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void TemplateDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTemplate = TemplateDataGrid.SelectedItem as DocumentTemplate;
            bool hasSelection = _selectedTemplate != null;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAddTemplate_Click(object sender, RoutedEventArgs e)
        {
            var window = new Views.Admin.EditWindow.EditDocumentTemplateWindow(null);
            if (window.ShowDialog() == true)
            {
                LoadTemplates(_currentSearchText);
            }
        }
        private void BtnEditTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTemplate == null) return;
            var window = new Views.Admin.EditWindow.EditDocumentTemplateWindow(_selectedTemplate.Id);
            if (window.ShowDialog() == true)
            {
                LoadTemplates(_currentSearchText);
            }
        }
        private void BtnDeleteTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTemplate == null) return;
            using var context = new ClinicSoftContext();
            bool isUsedInDocuments = context.Documents
                .Any(d => d.DocumentTemplateId == _selectedTemplate.Id);
            if (isUsedInDocuments)
            {
                MessageBox.Show(
                    "Невозможно удалить шаблон: он используется в существующих документах.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить шаблон:\n\"{_selectedTemplate.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    context.DocumentTemplates.Remove(_selectedTemplate);
                    context.SaveChanges();
                    LoadTemplates(_currentSearchText);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка при удалении шаблона:\n{ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}