using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminDiagnosisPage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 580;
        private Diagnosis _selectedDiagnosis;
        private string _currentSearchText = "";
        public AdminDiagnosisPage()
        {
            InitializeComponent();
            LoadDiagnoses();
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
        private void LoadDiagnoses(string searchTerm = "")
        {
            using var context = new ClinicSoftContext();
            var allDiagnoses = context.Diagnoses
                .OrderBy(d => d.Name)
                .ToList();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.Trim().ToLowerInvariant();
                allDiagnoses = allDiagnoses
                    .Where(d => d.Name != null &&
                               d.Name.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            DiagnosisDataGrid.ItemsSource = allDiagnoses;
        }
        private void DiagnosisDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedDiagnosis = DiagnosisDataGrid.SelectedItem as Diagnosis;
            bool hasSelection = _selectedDiagnosis != null;
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
                LoadDiagnoses(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void BtnAddDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            var window = new EditDiagnosisWindow(null);
            if (window.ShowDialog() == true)
            {
                LoadDiagnoses(_currentSearchText);
            }
        }
        private void BtnEditDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDiagnosis == null) return;
            var window = new EditDiagnosisWindow(_selectedDiagnosis.Id);
            if (window.ShowDialog() == true)
            {
                LoadDiagnoses(_currentSearchText);
            }
        }
        private void BtnDeleteDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDiagnosis == null) return;
            using var context = new ClinicSoftContext();
            var isUsed = context.Visits.Any(v => v.DiagnosisId == _selectedDiagnosis.Id);
            if (isUsed)
            {
                MessageBox.Show(
                    "Невозможно удалить диагноз, так как он используется в приёмах.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить диагноз:\n\"{_selectedDiagnosis.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (result == MessageBoxResult.Yes)
            {
                context.Diagnoses.Remove(_selectedDiagnosis);
                context.SaveChanges();
                LoadDiagnoses(_currentSearchText);
            }
        }
    }
}