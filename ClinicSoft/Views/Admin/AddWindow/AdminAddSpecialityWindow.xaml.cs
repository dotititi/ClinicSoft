using ClinicSoft.Data;
using System;
using System.Windows;

namespace ClinicSoft.Views.Admin.AddWindow
{
    public partial class AdminAddSpecialityWindow : Window
    {
        public AdminAddSpecialityWindow()
        {
            InitializeComponent();
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string specialityName = TxtName.Text?.Trim();
            if (string.IsNullOrWhiteSpace(specialityName))
            {
                MessageBox.Show("Введите название специальности.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                bool exists = context.MedicalSpecialities
                    .AsEnumerable()
                    .Any(s => s.Name != null &&
                             s.Name.Trim().Equals(specialityName, StringComparison.OrdinalIgnoreCase));
                if (exists)
                {
                    MessageBox.Show(
                        $"Специальность с названием «{specialityName}» уже существует.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                context.MedicalSpecialities.Add(new Models.MedicalSpeciality { Name = specialityName });
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