using ClinicSoft.Data;
using System;
using System.Windows;

namespace ClinicSoft.Views.Admin.EditWindow
{
    public partial class AdminEditDepartmentWindow : Window
    {
        private readonly int _departmentId;

        public AdminEditDepartmentWindow(int id, string name)
        {
            InitializeComponent();
            _departmentId = id;
            TxtName.Text = name;
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
                    .Any(d => d.Id != _departmentId &&
                             d.Name != null &&
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
                var department = context.Departments.Find(_departmentId);
                if (department != null)
                {
                    department.Name = departmentName;
                    context.SaveChanges();

                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Отделение не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}