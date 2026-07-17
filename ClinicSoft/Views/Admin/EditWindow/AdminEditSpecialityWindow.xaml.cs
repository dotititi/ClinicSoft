using ClinicSoft.Data;
using System;
using System.Windows;

namespace ClinicSoft.Views.Admin.EditWindow
{
    /// <summary>
    /// Логика взаимодействия для AdminEditSpecialityWindow.xaml
    /// </summary>
    public partial class AdminEditSpecialityWindow : Window
    {
        private readonly int _specialityId;
        public AdminEditSpecialityWindow(int id, string name)
        {
            InitializeComponent();
            _specialityId = id;
            TxtName.Text = name;
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
                    .Any(s => s.Id != _specialityId &&
                             s.Name != null &&
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
                var speciality = context.MedicalSpecialities.Find(_specialityId);
                if (speciality != null)
                {
                    speciality.Name = specialityName;
                    context.SaveChanges();

                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Специальность не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}