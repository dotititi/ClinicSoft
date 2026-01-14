using ClinicSoft.Data;
using ClinicSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClinicSoft.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для PatientPage.xaml
    /// </summary>
    public partial class PatientPage : Page
    {
        private ClinicSoftContext _context = new();
        private Patient _selectedPatient;

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
                    p.MiddleName.Contains(searchTerm));
            }

            var patients = query.ToList();
            // Добавляем вычисляемое поле FullName
            foreach (var p in patients)
                p.FullName = $"{p.LastName} {p.FirstName} {p.MiddleName}";

            PatientDataGrid.ItemsSource = patients;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadPatients(SearchBox.Text.Trim());
        }

        private void PatientDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPatient = PatientDataGrid.SelectedItem as Patient;
            // Можно открыть детали или разрешить редактирование
        }

        private void BtnAddPatient_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddPatientWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadPatients(); // Обновить список
            }
        }
    }
}
