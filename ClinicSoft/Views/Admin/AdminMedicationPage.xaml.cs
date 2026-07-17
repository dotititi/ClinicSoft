using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminMedicationPage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 560;
        private Medication _selectedMedication;
        private string _currentSearchText = "";
        public AdminMedicationPage()
        {
            InitializeComponent();
            LoadMedications();
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
        private void LoadMedications(string searchTerm = "")
        {
            using var context = new ClinicSoftContext();
            var allMedications = context.Medications
                .Include(m => m.DosageForm)
                .OrderBy(m => m.Name)
                .ToList();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.Trim().ToLowerInvariant();
                allMedications = allMedications
                    .Where(m => m.Name != null &&
                               m.Name.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            MedicationDataGrid.ItemsSource = allMedications;
        }
        private void MedicationDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedMedication = MedicationDataGrid.SelectedItem as Medication;
            bool hasSelection = _selectedMedication != null;
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
                LoadMedications(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void BtnAddMedication_Click(object sender, RoutedEventArgs e)
        {
            var window = new EditMedicationWindow(null);
            if (window.ShowDialog() == true)
            {
                LoadMedications(_currentSearchText);
            }
        }
        private void BtnEditMedication_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMedication == null) return;
            var window = new EditMedicationWindow(_selectedMedication.Id);
            if (window.ShowDialog() == true)
            {
                LoadMedications(_currentSearchText);
            }
        }
        private void BtnDeleteMedication_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMedication == null) return;
            using var context = new ClinicSoftContext();
            var isUsed = context.PrescribedMedications.Any(pm => pm.MedicationId == _selectedMedication.Id);
            if (isUsed)
            {
                MessageBox.Show(
                    "Невозможно удалить препарат, так как он используется в рецептах.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить препарат:\n\"{_selectedMedication.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (result == MessageBoxResult.Yes)
            {
                context.Medications.Remove(_selectedMedication);
                context.SaveChanges();
                LoadMedications(_currentSearchText);
            }
        }
    }
}