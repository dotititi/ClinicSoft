using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminGenderPage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 520;
        private Gender _selectedGender;
        private string _currentSearchText = "";
        public AdminGenderPage()
        {
            InitializeComponent();
            LoadGenders();
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
        private void LoadGenders(string searchTerm = "")
        {
            using var context = new ClinicSoftContext();
            var allGenders = context.Genders
                .OrderBy(g => g.Name)
                .ToList();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.Trim().ToLowerInvariant();
                allGenders = allGenders
                    .Where(g => g.Name != null &&
                               g.Name.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            GenderDataGrid.ItemsSource = allGenders;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadGenders(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void GenderDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedGender = GenderDataGrid.SelectedItem as Gender;
            bool hasSelection = _selectedGender != null;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAddGender_Click(object sender, RoutedEventArgs e)
        {
            var window = new Views.Admin.EditWindow.EditGenderWindow(null);
            if (window.ShowDialog() == true)
            {
                LoadGenders(_currentSearchText);
            }
        }
        private void BtnEditGender_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGender == null) return;
            var window = new Views.Admin.EditWindow.EditGenderWindow(_selectedGender.Id);
            if (window.ShowDialog() == true)
            {
                LoadGenders(_currentSearchText);
            }
        }
        private void BtnDeleteGender_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGender == null) return;
            using var context = new ClinicSoftContext();
            bool isUsed = context.Patients.Any(p => p.GenderCode == _selectedGender.Id) ||
                          context.Doctors.Any(d => d.GenderCode == _selectedGender.Id) ||
                          context.Admins.Any(a => a.GenderCode == _selectedGender.Id) ||
                          context.Registrators.Any(r => r.GenderCode == _selectedGender.Id);
            if (isUsed)
            {
                MessageBox.Show("Нельзя удалить пол: он используется пользователями.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить пол:\n\"{_selectedGender.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                context.Genders.Remove(_selectedGender);
                context.SaveChanges();
                LoadGenders(_currentSearchText);
            }
        }
    }
}