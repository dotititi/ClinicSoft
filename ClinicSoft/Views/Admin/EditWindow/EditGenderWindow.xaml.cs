using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace ClinicSoft.Views.Admin.EditWindow
{
    public partial class EditGenderWindow : Window
    {
        private readonly int? _id;
        public EditGenderWindow(int? id)
        {
            InitializeComponent();
            _id = id;
            if (_id.HasValue)
            {
                Title = "Редактирование пола";
                FormTitle.Text = "Редактирование пола";
                BtnSave.Content = "Сохранить";
                LoadData();
            }
            else
            {
                Title = "Добавление пола";
                FormTitle.Text = "Добавление нового пола";
                BtnSave.Content = "Добавить";
            }
        }
        private void LoadData()
        {
            using var context = new ClinicSoftContext();
            var gender = context.Genders.Find(_id.Value);
            if (gender != null)
            {
                TxtName.Text = gender.Name;
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var name = TxtName.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Название не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                string nameLower = name.ToLowerInvariant();
                if (_id.HasValue)
                {
                    bool duplicateExists = context.Genders
                        .AsEnumerable()
                        .Any(g => g.Id != _id.Value &&
                                 g.Name != null &&
                                 g.Name.Trim().ToLowerInvariant() == nameLower);
                    if (duplicateExists)
                    {
                        MessageBox.Show(
                            $"Пол «{name}» уже существует.",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    var gender = context.Genders.Find(_id.Value);
                    if (gender != null)
                    {
                        gender.Name = name;
                        context.SaveChanges();
                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    bool exists = context.Genders
                        .AsEnumerable()
                        .Any(g => g.Name != null &&
                                 g.Name.Trim().ToLowerInvariant() == nameLower);
                    if (exists)
                    {
                        MessageBox.Show(
                            $"Пол «{name}» уже существует.",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    context.Genders.Add(new Gender { Name = name });
                    context.SaveChanges();
                    DialogResult = true;
                    Close();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}