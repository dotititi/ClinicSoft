using ClinicSoft.Data;
using ClinicSoft.Models;
using ClinicSoft.Views.PatientViews;
using ClinicSoft.Views.Registrator;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Registrator
{
    /// <summary>
    /// Логика взаимодействия для PatientPage.xaml
    /// </summary>
    public partial class PatientPage : Page
    {
        public PatientPage()
        {
            InitializeComponent();
            LoadPatients();
        }
        private void LoadPatients()
        {
            if (PatientDataGrid == null)
                return;
            using var context = new ClinicSoftContext();
            var patients = context.Patients
                .Include(p => p.MedicalCard)
                .AsNoTracking()
                .ToList();
            string searchTerm = SearchBox?.Text?.Trim();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLowerInvariant();
                patients = patients.Where(p =>
                    p.LastName.ToLowerInvariant().Contains(searchTerm) ||
                    p.FirstName.ToLowerInvariant().Contains(searchTerm) ||
                    (p.MiddleName != null && p.MiddleName.ToLowerInvariant().Contains(searchTerm)) ||
                    (p.Phone != null && p.Phone.ToLowerInvariant().Contains(searchTerm))
                ).ToList();
            }
            PatientDataGrid.ItemsSource = patients;
        }
        private void BtnAddPatient_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddPatientWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadPatients();
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                ? Visibility.Visible : Visibility.Collapsed;
            LoadPatients();
        }
        private void BtnEditContact_Click(object sender, RoutedEventArgs e)
        {
            if (PatientDataGrid.SelectedItem is Patient selectedPatient)
            {
                using var context = new ClinicSoftContext();
                var patient = context.Patients
                    .Include(p => p.MedicalCard)
                    .FirstOrDefault(p => p.Id == selectedPatient.Id);
                if (patient == null)
                {
                    MessageBox.Show("Пациент не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var editWindow = new EditPatientContactWindow(
                    patient.Id,
                    patient.LastName,
                    patient.FirstName,
                    patient.MiddleName,
                    patient.Phone,
                    patient.Email,
                    patient.MedicalCard?.InsuranceNumber ?? "Не указано",
                    patient.MedicalCard?.Allergies ?? "Не указано",
                    patient.MedicalCard?.ChronicConditions ?? "Не указано"
                );
                if (editWindow.ShowDialog() == true)
                {
                    LoadPatients();
                }
            }
        }
        private void PatientDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnEditContact.IsEnabled = (PatientDataGrid.SelectedItem != null);
        }
        private void BtnViewMedicalCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int patientId)
            {
                var parentWindow = Window.GetWindow(this) as RegistratorWindow;
                parentWindow?.MainFrame.Navigate(new MedicalCardPage(patientId));
            }
        }
        private void BtnViewLabTests_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int patientId)
            {
                var parentWindow = Window.GetWindow(this) as RegistratorWindow;
                parentWindow?.MainFrame.Navigate(new RegistratorLabTestsPage(patientId));
            }
        }
        private void BtnViewDocuments_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int patientId)
            {
                var parentWindow = Window.GetWindow(this) as RegistratorWindow;
                parentWindow?.MainFrame.Navigate(new RegistratorDocumentsPage(patientId));
            }
        }
        private void BtnViewTreatments_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int patientId)
            {
                var parentWindow = Window.GetWindow(this) as RegistratorWindow;
                parentWindow?.MainFrame.Navigate(new RegistratorTreatmentPage(patientId));
            }
        }
    }
}