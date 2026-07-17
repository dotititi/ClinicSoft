using ClinicSoft.Data;
using ClinicSoft.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminDocumentTypePage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 620;
        private DocumentType _selectedType;
        private string _currentSearchText = "";
        public AdminDocumentTypePage()
        {
            InitializeComponent();
            LoadTypes();
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
        private void LoadTypes(string searchTerm = "")
        {
            using var context = new ClinicSoftContext();
            var allTypes = context.DocumentTypes
                .OrderBy(t => t.Name)
                .ToList();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.Trim().ToLowerInvariant();
                allTypes = allTypes
                    .Where(t => t.Name != null &&
                               t.Name.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            TypeDataGrid.ItemsSource = allTypes;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadTypes(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void TypeDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedType = TypeDataGrid.SelectedItem as DocumentType;
            bool hasSelection = _selectedType != null;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAddType_Click(object sender, RoutedEventArgs e)
        {
            var window = new EditWindow.EditDocumentTypeWindow(null);
            if (window.ShowDialog() == true)
            {
                LoadTypes(_currentSearchText);
            }
        }
        private void BtnEditType_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedType == null) return;
            var window = new EditWindow.EditDocumentTypeWindow(_selectedType.Id);
            if (window.ShowDialog() == true)
            {
                LoadTypes(_currentSearchText);
            }
        }
        private void BtnDeleteType_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedType == null) return;
            using var context = new ClinicSoftContext();
            var isUsed = context.DocumentTemplates.Any(t => t.DocumentTypeId == _selectedType.Id);
            if (isUsed)
            {
                MessageBox.Show("Нельзя удалить тип документа: существуют шаблоны этого типа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить тип документа:\n\"{_selectedType.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                context.DocumentTypes.Remove(_selectedType);
                context.SaveChanges();
                LoadTypes(_currentSearchText);
            }
        }
    }
}