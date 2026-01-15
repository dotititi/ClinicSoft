using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            // Врачи: формируем отображаемое имя на лету
            var doctors = _context.Doctors
                .Include(d => d.Speciality)
                .Include(d => d.Department)
                .Select(d => new
                {
                    Id = d.Id,
                    FullName = $"{d.LastName} {d.FirstName} {d.MiddleName}".Trim(),
                    Speciality = d.Speciality != null ? d.Speciality.Name : "Не указано",
                    Department = d.Department != null ? d.Department.Name : "Не указано"
                })
                .ToList();

            DoctorGrid.ItemsSource = doctors;

            // Отделения: формируем ФИО заведующего
            var departments = _context.Departments
                .Include(d => d.HeadDoctor)
                    .ThenInclude(h => h.User)
                .Select(d => new
                {
                    Id = d.Id,
                    Name = d.Name,
                    HeadDoctorFullName = d.HeadDoctor != null
                        ? $"{d.HeadDoctor.LastName} {d.HeadDoctor.FirstName} {d.HeadDoctor.MiddleName}".Trim()
                        : "Не назначен"
                })
                .ToList();

            DepartmentGrid.ItemsSource = departments;
        }

        private void BtnAddDoctor_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления врача — в разработке.");
        }

        private void BtnAddDepartment_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления отделения — в разработке.");
        }

        private void BtnSpecialities_Click(object sender, RoutedEventArgs e)
        {
            var specWindow = new SpecialityWindow();
            specWindow.ShowDialog();
            LoadData();
        }
    }
}