using ClinicSoft.Data;
using ClinicSoft.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminOfficePage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 520;
        private Office _selectedOffice;
        private string _currentSearchText = "";
        public AdminOfficePage()
        {
            InitializeComponent();
            LoadOffices();
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
        private void LoadOffices(string searchTerm = "")
        {
            using var context = new ClinicSoftContext();
            var allOffices = context.Offices
                .OrderBy(o => o.Number)
                .ToList();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.Trim().ToLowerInvariant();
                allOffices = allOffices
                    .Where(o => o.Number != null &&
                               o.Number.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            OfficeDataGrid.ItemsSource = allOffices;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadOffices(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void OfficeDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedOffice = OfficeDataGrid.SelectedItem as Office;
            bool hasSelection = _selectedOffice != null;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAddOffice_Click(object sender, RoutedEventArgs e)
        {
            var window = new Views.Admin.EditWindow.EditOfficeWindow(null);
            if (window.ShowDialog() == true)
            {
                LoadOffices(_currentSearchText);
            }
        }
        private void BtnEditOffice_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOffice == null) return;
            var window = new Views.Admin.EditWindow.EditOfficeWindow(_selectedOffice.Id);
            if (window.ShowDialog() == true)
            {
                LoadOffices(_currentSearchText);
            }
        }
        private void BtnDeleteOffice_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOffice == null) return;
            using var context = new ClinicSoftContext();
            var isUsedByDoctors = context.Doctors.Any(d => d.OfficeId == _selectedOffice.Id);
            if (isUsedByDoctors)
            {
                MessageBox.Show("Нельзя удалить кабинет: он закреплен за врачем.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить кабинет:\n\"{_selectedOffice.Number}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                context.Offices.Remove(_selectedOffice);
                context.SaveChanges();
                LoadOffices(_currentSearchText);
            }
        }
    }
}