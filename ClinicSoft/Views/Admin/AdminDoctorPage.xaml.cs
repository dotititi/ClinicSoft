using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminDoctorPage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 650;
        private Doctor _selectedDoctor;
        private string _currentSearchText = "";

        public AdminDoctorPage()
        {
            InitializeComponent();
            LoadDoctors();
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

        private void LoadDoctors(string search = null)
        {
            using var context = new ClinicSoftContext();
            var allDoctors = context.Doctors
                .Include(d => d.Speciality)
                .Include(d => d.Department)
                .Include(d => d.Office)
                .Include(d => d.Status)
                .Include(d => d.User)
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName)
                .ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.Trim().ToLowerInvariant();
                allDoctors = allDoctors
                    .Where(d =>
                        // Поиск по ФИО
                        (!string.IsNullOrEmpty(d.LastName) && d.LastName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(d.FirstName) && d.FirstName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(d.MiddleName) && d.MiddleName.ToLowerInvariant().Contains(searchLower)) ||
                        // Поиск по логину
                        (d.User?.Login != null && d.User.Login.ToLowerInvariant().Contains(searchLower)) ||
                        // Поиск по почте
                        (!string.IsNullOrEmpty(d.Email) && d.Email.ToLowerInvariant().Contains(searchLower)))
                    .ToList();
            }

            DoctorDataGrid.ItemsSource = allDoctors;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadDoctors(_currentSearchText);
            }
        }

        private void UpdateSearchDisplay()
        {
            // Синхронизируем обе поисковые строки
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;

            // Обновляем placeholders
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DoctorDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedDoctor = DoctorDataGrid.SelectedItem as Doctor;
            bool hasSelection = _selectedDoctor != null;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }

        private void BtnAddDoctor_Click(object sender, RoutedEventArgs e)
        {
            var window = new Views.Admin.AddWindow.AdminAddDoctorWindow();
            if (window.ShowDialog() == true)
            {
                LoadDoctors(_currentSearchText);
            }
        }

        private void BtnEditDoctor_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDoctor == null) return;
            var editWindow = new Views.Admin.EditWindow.AdminEditDoctorWindow(_selectedDoctor.Id);
            if (editWindow.ShowDialog() == true)
            {
                LoadDoctors(_currentSearchText);
            }
        }

        private void BtnDeleteDoctor_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDoctor == null)
            {
                MessageBox.Show("Выберите врача для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string fullName = $"{_selectedDoctor.LastName} {_selectedDoctor.FirstName} {_selectedDoctor.MiddleName}".Trim();
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить врача:\n{fullName}?\n\n" +
                "Внимание: Удаление невозможно, если у врача есть связанные записи (приёмы, анализы, назначения).",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new ClinicSoftContext();
                    var doctor = context.Doctors.Find(_selectedDoctor.Id);
                    if (doctor == null)
                    {
                        MessageBox.Show("Врач не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    bool hasAppointments = context.Appointments.Any(a => a.DoctorId == doctor.Id);
                    bool hasVisits = context.Visits.Any(v => v.DoctorId == doctor.Id);
                    bool hasPrescriptions = context.TreatmentPlans.Any(p => p.DoctorId == doctor.Id);
                    bool hasLabOrders = context.LabOrders.Any(lo => lo.DoctorId == doctor.Id);
                    bool hasDocuments = context.Documents.Any(d => d.DoctorId == doctor.Id);
                    var relatedEntities = new System.Collections.Generic.List<string>();
                    if (hasAppointments) relatedEntities.Add("записи на приём");
                    if (hasVisits) relatedEntities.Add("визиты");
                    if (hasPrescriptions) relatedEntities.Add("назначения");
                    if (hasLabOrders) relatedEntities.Add("анализы");
                    if (hasDocuments) relatedEntities.Add("документы");
                    if (relatedEntities.Any())
                    {
                        string entitiesList = string.Join(", ", relatedEntities);
                        MessageBox.Show(
                            $"Нельзя удалить врача \"{fullName}\"\n\n" +
                            $"Найдены связанные записи: {entitiesList}.\n\n" +
                            "Сначала удалите или переназначьте все связанные записи другому врачу.",
                            "Удаление невозможно",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                    context.Doctors.Remove(doctor);
                    context.SaveChanges();
                    LoadDoctors(_currentSearchText);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка при удалении врача:\n{ex.Message}",
                        "Критическая ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}