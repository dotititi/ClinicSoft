using ClinicSoft.Data;
using ClinicSoft.Models;
using ClinicSoft.Views.Registrator;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminPatientPage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 700;
        private string _currentSearchText = "";
        public AdminPatientPage()
        {
            InitializeComponent();
            LoadPatients();
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
        private void LoadPatients(string search = null)
        {
            using var context = new ClinicSoftContext();
            var allPatients = context.Patients
                .Include(p => p.MedicalCard)
                .Include(p => p.GenderCodeNavigation)
                .Include(p => p.User)
                .ToList();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.Trim().ToLowerInvariant();
                allPatients = allPatients
                    .Where(p =>
                        (!string.IsNullOrEmpty(p.LastName) && p.LastName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(p.FirstName) && p.FirstName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(p.MiddleName) && p.MiddleName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(p.Phone) && p.Phone.Contains(search)) ||
                        (!string.IsNullOrEmpty(p.Email) && p.Email.ToLowerInvariant().Contains(searchLower)) ||
                        (p.User?.Login != null && p.User.Login.ToLowerInvariant().Contains(searchLower)) ||
                        (p.MedicalCard?.InsuranceNumber != null && p.MedicalCard.InsuranceNumber.Contains(search)))
                    .ToList();
            }
            PatientDataGrid.ItemsSource = allPatients;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadPatients(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void PatientDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = PatientDataGrid.SelectedItem is Patient;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAddPatient_Click(object sender, RoutedEventArgs e)
        {
            var window = new Views.Admin.AddWindow.AddPatientWindow();
            if (window.ShowDialog() == true)
            {
                LoadPatients(_currentSearchText);
            }
        }
        private void BtnEditPatient_Click(object sender, RoutedEventArgs e)
        {
            if (PatientDataGrid.SelectedItem is not Patient selected)
            {
                MessageBox.Show("Выберите пациента.");
                return;
            }
            var editWindow = new Views.Admin.EditWindow.AdminEditPatientWindow(selected.Id);
            if (editWindow.ShowDialog() == true)
            {
                LoadPatients(_currentSearchText);
            }
        }
        private void BtnDeletePatient_Click(object sender, RoutedEventArgs e)
        {
            if (PatientDataGrid.SelectedItem is not Patient selected)
            {
                MessageBox.Show("Выберите пациента для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string fullName = $"{selected.LastName} {selected.FirstName} {selected.MiddleName}".Trim();
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить пациента:\n{fullName}?\n\n" +
                "Внимание: Удаление невозможно, если у пациента есть связанные записи (записи на приём, анализы, назначения и т.д.).",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new ClinicSoftContext();
                    var patient = context.Patients.Find(selected.Id);
                    if (patient == null)
                    {
                        MessageBox.Show("Пациент не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    bool hasAppointments = context.Appointments.Any(a => a.PatientId == patient.Id);
                    bool hasVisits = context.Visits.Any(v => v.PatientId == patient.Id);
                    bool hasLabOrders = context.LabOrders.Any(lo => lo.PatientId == patient.Id);
                    bool hasPrescriptions = context.TreatmentPlans.Any(p => p.Visit.PatientId == patient.Id);
                    bool hasDocuments = context.Documents.Any(d => d.PatientId == patient.Id);
                    var relatedEntities = new System.Collections.Generic.List<string>();
                    if (hasAppointments) relatedEntities.Add("записи на приём");
                    if (hasVisits) relatedEntities.Add("визиты");
                    if (hasLabOrders) relatedEntities.Add("анализы");
                    if (hasPrescriptions) relatedEntities.Add("назначения");
                    if (hasDocuments) relatedEntities.Add("документы");
                    if (relatedEntities.Any())
                    {
                        string entitiesList = string.Join(", ", relatedEntities);
                        MessageBox.Show(
                            $"Нельзя удалить пациента \"{fullName}\"\n\n" +
                            $"Найдены связанные записи: {entitiesList}.\n\n" +
                            "Сначала удалите или закройте все связанные записи.",
                            "Удаление невозможно",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                    var medicalCard = context.MedicalCards.FirstOrDefault(m => m.PatientId == patient.Id);
                    if (medicalCard != null)
                        context.MedicalCards.Remove(medicalCard);
                    context.Patients.Remove(patient);
                    context.SaveChanges();
                    MessageBox.Show(
                        $"Пациент \"{fullName}\" успешно удалён.",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    LoadPatients(_currentSearchText);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка при удалении пациента:\n{ex.Message}",
                        "Критическая ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}