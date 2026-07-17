using ClinicSoft.Data;
using ClinicSoft.Models;
using ClinicSoft.Views.Admin.AddWindow;
using ClinicSoft.Views.Admin.EditWindow;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminLabTestTypePage : Page
    {
        private class LabTestTypeItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? NormalRange { get; set; }
            public string? UnitSymbol { get; set; }
        }
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 600;
        private string _currentSearchText = "";
        public AdminLabTestTypePage()
        {
            InitializeComponent();
            LoadLabTestTypes();
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
        private void LoadLabTestTypes(string search = null)
        {
            using var context = new ClinicSoftContext();
            var allLabTestTypes = context.LabTestTypes
                .Include(ltt => ltt.Unit)
                .Select(l => new LabTestTypeItem
                {
                    Id = l.Id,
                    Name = l.Name,
                    NormalRange = l.NormalRange,
                    UnitSymbol = l.Unit != null ? l.Unit.Symbol : "—"
                })
                .OrderBy(l => l.Name)
                .ToList();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.Trim().ToLowerInvariant();
                allLabTestTypes = allLabTestTypes
                    .Where(l => l.Name != null &&
                               l.Name.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            LabTestTypeDataGrid.ItemsSource = allLabTestTypes;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadLabTestTypes(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void LabTestTypeDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = LabTestTypeDataGrid.SelectedItem is LabTestTypeItem;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAddLabTestType_Click(object sender, RoutedEventArgs e)
        {
            var window = new AdminAddLabTestTypeWindow();
            if (window.ShowDialog() == true)
            {
                LoadLabTestTypes(_currentSearchText);
            }
        }
        private void BtnEditLabTestType_Click(object sender, RoutedEventArgs e)
        {
            if (LabTestTypeDataGrid.SelectedItem is not LabTestTypeItem selected) return;

            var editWindow = new AdminEditLabTestTypeWindow(selected.Id, selected.Name);
            if (editWindow.ShowDialog() == true)
            {
                LoadLabTestTypes(_currentSearchText);
            }
        }
        private void BtnDeleteLabTestType_Click(object sender, RoutedEventArgs e)
        {
            if (LabTestTypeDataGrid.SelectedItem is not LabTestTypeItem selected)
            {
                MessageBox.Show("Выберите тип анализа для удаления.");
                return;
            }
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить тип анализа:\n{selected.Name}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new ClinicSoftContext();
                    bool hasOrders = context.LabOrderItems.Any(item => item.TestTypeId == selected.Id);
                    if (hasOrders)
                    {
                        MessageBox.Show("Нельзя удалить тип анализа: существуют анализы с этим типом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var labTestType = context.LabTestTypes.Find(selected.Id);
                    if (labTestType != null)
                    {
                        context.LabTestTypes.Remove(labTestType);
                        context.SaveChanges();
                        LoadLabTestTypes(_currentSearchText);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}