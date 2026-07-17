using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace ClinicSoft.Views.Admin.AddWindow
{
    public partial class AdminAddDepartmentWindow : Window
    {
        public AdminAddDepartmentWindow()
        {
            InitializeComponent();
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string departmentName = TxtName.Text?.Trim();
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                MessageBox.Show("Введите название отделения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();

                bool exists = context.Departments
                    .AsEnumerable() 
                    .Any(d => d.Name != null &&
                     d.Name.Trim().Equals(departmentName, StringComparison.OrdinalIgnoreCase));
                if (exists)
                {
                    MessageBox.Show(
                        $"Отделение с названием «{departmentName}» уже существует.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                context.Departments.Add(new Models.Department { Name = departmentName });
                context.SaveChanges();    
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}