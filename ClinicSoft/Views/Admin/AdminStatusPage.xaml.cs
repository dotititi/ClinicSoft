using ClinicSoft.Data;
using ClinicSoft.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminStatusPage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 540;
        private DoctorStatus _selectedStatus;
        private string _currentSearchText = "";
        public AdminStatusPage()
        {
            InitializeComponent();
            LoadStatuses();
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
        private void LoadStatuses(string searchTerm = "")
        {
            using var context = new ClinicSoftContext();
            var allStatuses = context.DoctorStatuses
                .OrderBy(s => s.Name)
                .ToList();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.Trim().ToLowerInvariant();
                allStatuses = allStatuses
                    .Where(s => s.Name != null &&
                               s.Name.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            StatusDataGrid.ItemsSource = allStatuses;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadStatuses(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void StatusDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedStatus = StatusDataGrid.SelectedItem as DoctorStatus;
            bool hasSelection = _selectedStatus != null;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAddStatus_Click(object sender, RoutedEventArgs e)
        {
            var window = new Views.Admin.EditWindow.EditStatusWindow(null);
            if (window.ShowDialog() == true)
            {
                LoadStatuses(_currentSearchText);
            }
        }
        private void BtnEditStatus_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStatus == null) return;
            var window = new Views.Admin.EditWindow.EditStatusWindow(_selectedStatus.Id);
            if (window.ShowDialog() == true)
            {
                LoadStatuses(_currentSearchText);
            }
        }
        private void BtnDeleteStatus_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStatus == null) return;
            if (_selectedStatus.Id <= 3)
            {
                MessageBox.Show("Нельзя удалить системный статус.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using var context = new ClinicSoftContext();
            var isUsed = context.Doctors.Any(d => d.StatusId == _selectedStatus.Id);
            if (isUsed)
            {
                MessageBox.Show("Нельзя удалить статус: он используется врачами.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить статус:\n\"{_selectedStatus.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                context.DoctorStatuses.Remove(_selectedStatus);
                context.SaveChanges();
                LoadStatuses(_currentSearchText);
            }
        }
    }
}