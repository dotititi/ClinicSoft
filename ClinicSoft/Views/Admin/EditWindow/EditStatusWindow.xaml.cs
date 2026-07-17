using ClinicSoft.Data;
using ClinicSoft.Models;
using System.Windows;

namespace ClinicSoft.Views.Admin.EditWindow
{
    public partial class EditStatusWindow : Window
    {
        private readonly int? _id;
        public EditStatusWindow(int? id)
        {
            InitializeComponent();
            _id = id;
            if (_id.HasValue)
            {
                Title = "Редактирование статуса врача";
                FormTitle.Text = "Редактирование статуса врача";
                BtnSave.Content = "Сохранить";
                LoadData();
            }
            else
            {
                Title = "Добавление статуса врача";
                FormTitle.Text = "Добавление нового статуса врача";
                BtnSave.Content = "Добавить";
            }
        }
        private void LoadData()
        {
            using var context = new ClinicSoftContext();
            var status = context.DoctorStatuses.Find(_id.Value);
            if (status != null)
            {
                TxtName.Text = status.Name;
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var name = TxtName.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Название статуса не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                string nameLower = name.ToLowerInvariant();
                if (_id.HasValue)
                {
                    bool duplicateExists = context.DoctorStatuses
                        .AsEnumerable()
                        .Any(s => s.Id != _id.Value &&
                                 s.Name != null &&
                                 s.Name.Trim().ToLowerInvariant() == nameLower);
                    if (duplicateExists)
                    {
                        MessageBox.Show("Статус с таким названием уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    var status = context.DoctorStatuses.Find(_id.Value);
                    if (status != null)
                    {
                        status.Name = name;
                        context.SaveChanges();
                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    bool exists = context.DoctorStatuses
                        .AsEnumerable()
                        .Any(s => s.Name != null &&
                                 s.Name.Trim().ToLowerInvariant() == nameLower);
                    if (exists)
                    {
                        MessageBox.Show("Статус с таким названием уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    context.DoctorStatuses.Add(new DoctorStatus { Name = name });
                    context.SaveChanges();
                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.InnerException?.Message ?? ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}