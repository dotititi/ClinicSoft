using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore; // ← ключевая строка!
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class PatientPage : Page
    {
        private ClinicSoftContext _context = new();
        private global::ClinicSoft.Models.Patient _selectedPatient;

        public PatientPage()
        {
            InitializeComponent();
            LoadPatients();
        }

        private void LoadPatients(string searchTerm = null)
        {
            var query = _context.Patients
                .Include(p => p.MedicalCard)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p =>
                    p.FirstName.Contains(searchTerm) ||
                    p.LastName.Contains(searchTerm) ||
                    (p.MiddleName != null && p.MiddleName.Contains(searchTerm)));
            }

            PatientDataGrid.ItemsSource = query.ToList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadPatients(SearchBox.Text.Trim());
        }

        private void PatientDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPatient = PatientDataGrid.SelectedItem as ClinicSoft.Models.Patient;
        }

        private void BtnAddPatient_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddPatientWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadPatients();
            }
        }
    }
}