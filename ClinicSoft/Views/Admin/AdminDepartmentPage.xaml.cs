using ClinicSoft.Data;
using ClinicSoft.Models;
using ClinicSoft.Views.Admin.AddWindow;
using ClinicSoft.Views.Admin.EditWindow;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminDepartmentPage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 600;
        private string _currentSearchText = "";
        public AdminDepartmentPage()
        {
            InitializeComponent();
            LoadDepartments();
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
        private void LoadDepartments(string search = null)
        {
            using var context = new ClinicSoftContext();
            var allDepartments = context.Departments
                .OrderBy(d => d.Name)
                .ToList();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.Trim().ToLowerInvariant();
                allDepartments = allDepartments
                    .Where(d => d.Name != null &&
                               d.Name.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            DepartmentDataGrid.ItemsSource = allDepartments;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadDepartments(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void DepartmentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = DepartmentDataGrid.SelectedItem != null;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAddDepartment_Click(object sender, RoutedEventArgs e)
        {
            var window = new AdminAddDepartmentWindow();
            if (window.ShowDialog() == true)
            {
                LoadDepartments(_currentSearchText);
            }
        }
        private void BtnEditDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (DepartmentDataGrid.SelectedItem is not Department selected) return;
            var editWindow = new AdminEditDepartmentWindow(selected.Id, selected.Name);
            if (editWindow.ShowDialog() == true)
            {
                LoadDepartments(_currentSearchText);
            }
        }
        private void BtnDeleteDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (DepartmentDataGrid.SelectedItem is not Department selected)
            {
                MessageBox.Show("Выберите отделение для удаления.");
                return;
            }
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить отделение:\n{selected.Name}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new ClinicSoftContext();
                    bool hasDoctors = context.Doctors.Any(d => d.DepartmentId == selected.Id);
                    if (hasDoctors)
                    {
                        MessageBox.Show("Нельзя удалить отделение: к нему привязаны врачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var department = context.Departments.Find(selected.Id);
                    if (department != null)
                    {
                        context.Departments.Remove(department);
                        context.SaveChanges();
                        LoadDepartments(_currentSearchText);
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