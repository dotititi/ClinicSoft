using ClinicSoft.Data;
using ClinicSoft.Views.Admin.AddWindow;
using ClinicSoft.Views.Admin.EditWindow;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminSpecialityPage : Page
    {
        private class SpecialityItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
        }

        private const double MIN_WIDTH_FOR_SINGLE_LINE = 620;
        private string _currentSearchText = "";
        public AdminSpecialityPage()
        {
            InitializeComponent();
            LoadSpecialities();
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
        private void LoadSpecialities(string search = null)
        {
            using var context = new ClinicSoftContext();
            var allSpecialities = context.MedicalSpecialities
                .Select(s => new SpecialityItem { Id = s.Id, Name = s.Name })
                .OrderBy(s => s.Name)
                .ToList();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.Trim().ToLowerInvariant();
                allSpecialities = allSpecialities
                    .Where(s => s.Name != null &&
                               s.Name.ToLowerInvariant().Contains(searchLower))
                    .ToList();
            }
            SpecialityDataGrid.ItemsSource = allSpecialities;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadSpecialities(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void SpecialityDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = SpecialityDataGrid.SelectedItem is SpecialityItem;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAddSpeciality_Click(object sender, RoutedEventArgs e)
        {
            var window = new AdminAddSpecialityWindow();
            if (window.ShowDialog() == true)
            {
                LoadSpecialities(_currentSearchText);
            }
        }
        private void BtnEditSpeciality_Click(object sender, RoutedEventArgs e)
        {
            if (SpecialityDataGrid.SelectedItem is not SpecialityItem selected) return;

            var editWindow = new AdminEditSpecialityWindow(selected.Id, selected.Name);
            if (editWindow.ShowDialog() == true)
            {
                LoadSpecialities(_currentSearchText);
            }
        }
        private void BtnDeleteSpeciality_Click(object sender, RoutedEventArgs e)
        {
            if (SpecialityDataGrid.SelectedItem is not SpecialityItem selected)
            {
                MessageBox.Show("Выберите специальность для удаления.");
                return;
            }
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить специальность:\n{selected.Name}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new ClinicSoftContext();
                    bool hasDoctors = context.Doctors.Any(d => d.SpecialityId == selected.Id);
                    if (hasDoctors)
                    {
                        MessageBox.Show("Нельзя удалить специальность: к ней привязаны врачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var speciality = context.MedicalSpecialities.Find(selected.Id);
                    if (speciality != null)
                    {
                        context.MedicalSpecialities.Remove(speciality);
                        context.SaveChanges();
                        LoadSpecialities(_currentSearchText);
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