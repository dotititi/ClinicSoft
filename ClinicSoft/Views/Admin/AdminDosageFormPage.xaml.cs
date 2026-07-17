using ClinicSoft.Data;
using ClinicSoft.Models;
using ClinicSoft.Views.Admin.EditWindow;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminDosageFormPage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 680;
        private string _currentSearchText = "";
        public AdminDosageFormPage()
        {
            InitializeComponent();
            LoadDosageForms();
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
        private void LoadDosageForms(string searchTerm = "")
        {
            using var context = new ClinicSoftContext();
            var allDosageForms = context.DosageForms
                .OrderBy(df => df.Name)
                .ToList();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.Trim().ToLowerInvariant();
                allDosageForms = allDosageForms
                    .Where(df => df.Name != null &&
                                df.Name.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            DosageFormDataGrid.ItemsSource = allDosageForms;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadDosageForms(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void DosageFormDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = DosageFormDataGrid.SelectedItem as DosageForm;
            bool hasSelection = selected != null;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new EditDosageFormWindow(null);
            if (window.ShowDialog() == true)
            {
                LoadDosageForms(_currentSearchText);
            }
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = DosageFormDataGrid.SelectedItem as DosageForm;
            if (selected == null) return;
            var window = new EditDosageFormWindow(selected.Id);
            if (window.ShowDialog() == true)
            {
                LoadDosageForms(_currentSearchText);
            }
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = DosageFormDataGrid.SelectedItem as DosageForm;
            if (selected == null) return;
            using var context = new ClinicSoftContext();
            var isUsed = context.Medications.Any(m => m.DosageFormId == selected.Id);
            if (isUsed)
            {
                MessageBox.Show(
                    "Невозможно удалить форму дозировки, так как она используется в препаратах.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить форму дозировки:\n\"{selected.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (result == MessageBoxResult.Yes)
            {
                context.DosageForms.Remove(selected);
                context.SaveChanges();
                LoadDosageForms(_currentSearchText);
            }
        }
    }
}