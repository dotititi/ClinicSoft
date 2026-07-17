using ClinicSoft.Data;
using ClinicSoft.Models;
using System.Windows;

namespace ClinicSoft.Views.Admin.EditWindow
{
    public partial class EditOfficeWindow : Window
    {
        private readonly int? _id;
        public EditOfficeWindow(int? id)
        {
            InitializeComponent();
            _id = id;
            if (_id.HasValue)
            {
                Title = "Редактирование кабинета";
                FormTitle.Text = "Редактирование кабинета";
                BtnSave.Content = "Сохранить";
                LoadData();
            }
            else
            {
                Title = "Добавление кабинета";
                FormTitle.Text = "Добавление нового кабинета";
                BtnSave.Content = "Добавить";
            }
        }
        private void LoadData()
        {
            using var context = new ClinicSoftContext();
            var office = context.Offices.Find(_id.Value);
            if (office != null)
            {
                TxtNumber.Text = office.Number;
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var number = TxtNumber.Text?.Trim();
            if (string.IsNullOrWhiteSpace(number))
            {
                MessageBox.Show("Номер кабинета не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                string numberLower = number.ToLowerInvariant();
                if (_id.HasValue)
                {
                    bool duplicateExists = context.Offices
                        .AsEnumerable()
                        .Any(o => o.Id != _id.Value &&
                                 o.Number != null &&
                                 o.Number.Trim().ToLowerInvariant() == numberLower);
                    if (duplicateExists)
                    {
                        MessageBox.Show("Кабинет с таким номером уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    var office = context.Offices.Find(_id.Value);
                    if (office != null)
                    {
                        office.Number = number;
                        context.SaveChanges();
                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    bool exists = context.Offices
                        .AsEnumerable()
                        .Any(o => o.Number != null &&
                                 o.Number.Trim().ToLowerInvariant() == numberLower);
                    if (exists)
                    {
                        MessageBox.Show("Кабинет с таким номером уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    context.Offices.Add(new Office { Number = number });
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