using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminUnitOfMeasurementPage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 660;
        private UnitsOfMeasurement _selectedUnit;
        private string _currentSearchText = "";
        public AdminUnitOfMeasurementPage()
        {
            InitializeComponent();
            LoadUnits();
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
        private void LoadUnits(string searchTerm = "")
        {
            using var context = new ClinicSoftContext();
            var allUnits = context.UnitsOfMeasurements
                .OrderBy(u => u.Symbol)
                .ToList();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.Trim().ToLowerInvariant();
                allUnits = allUnits
                    .Where(u => u.Symbol != null &&
                               u.Symbol.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            UnitDataGrid.ItemsSource = allUnits;
        }
        private void UnitDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedUnit = UnitDataGrid.SelectedItem as UnitsOfMeasurement;
            bool hasSelection = _selectedUnit != null;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadUnits(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void BtnAddUnit_Click(object sender, RoutedEventArgs e)
        {
            var window = new EditUnitOfMeasurementWindow(null);
            if (window.ShowDialog() == true)
            {
                LoadUnits(_currentSearchText);
            }
        }
        private void BtnEditUnit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUnit == null) return;
            var window = new EditUnitOfMeasurementWindow(_selectedUnit.Id);
            if (window.ShowDialog() == true)
            {
                LoadUnits(_currentSearchText);
            }
        }
        private void BtnDeleteUnit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUnit == null) return;
            using var context = new ClinicSoftContext();
            var isUsed = context.LabResultItems
                    .Any(r => r.TestType != null && r.TestType.UnitId == _selectedUnit.Id);
            if (isUsed)
            {
                MessageBox.Show(
                    "Невозможно удалить единицу измерения, так как она используется в результатах анализов.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить единицу измерения:\n\"{_selectedUnit.Symbol}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (result == MessageBoxResult.Yes)
            {
                context.UnitsOfMeasurements.Remove(_selectedUnit);
                context.SaveChanges();
                LoadUnits(_currentSearchText);
            }
        }
    }
}