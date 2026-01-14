using ClinicSoft.Data;
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
    /// Логика взаимодействия для ReferencePage.xaml
    /// </summary>
    public partial class ReferencePage : Page
    {
        private ClinicSoftContext _context = new();

        public ReferencePage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            // Врачи
            var doctors = _context.Doctors
                .Include(d => d.Speciality)
                .Include(d => d.Department)
                .ToList();
            foreach (var d in doctors) d.FullName = $"{d.LastName} {d.FirstName} {d.MiddleName}";
            DoctorGrid.ItemsSource = doctors;

            // Отделения
            var departments = _context.Departments
                .Include(d => d.HeadDoctor)
                    .ThenInclude(h => h.User)
                .ToList();
            foreach (var d in departments)
            {
                if (d.HeadDoctor != null)
                    d.HeadDoctorFullName = $"{d.HeadDoctor.LastName} {d.HeadDoctor.FirstName} {d.HeadDoctor.MiddleName}";
            }
            DepartmentGrid.ItemsSource = departments;
        }

        private void BtnAddDoctor_Click(object sender, RoutedEventArgs e)
        {
            // Можно открыть отдельное окно AddDoctorWindow (по аналогии с пациентом)
            MessageBox.Show("Функция добавления врача — в разработке.");
        }

        private void BtnAddDepartment_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления отделения — в разработке.");
        }

        private void BtnSpecialities_Click(object sender, RoutedEventArgs e)
        {
            // Открыть список специальностей
            var specWindow = new SpecialityWindow();
            specWindow.ShowDialog();
            LoadData(); // Обновить данные
        }
    }
}
