using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace ClinicSoft.Views.Admin.EditWindow
{
    public partial class EditDosageFormWindow : Window
    {
        private readonly int? _id;
        public EditDosageFormWindow(int? id)
        {
            InitializeComponent();
            _id = id;
            if (_id.HasValue)
            {
                Title = "Редактирование формы дозировки";
                FormTitle.Text = "Редактирование формы дозировки";
                BtnSave.Content = "Сохранить";
                LoadData();
            }
            else
            {
                Title = "Добавление формы дозировки";
                FormTitle.Text = "Добавление новой формы дозировки";
                BtnSave.Content = "Добавить";
            }
        }
        private void LoadData()
        {
            using var context = new ClinicSoftContext();
            var dosageForm = context.DosageForms.Find(_id.Value);
            if (dosageForm != null)
            {
                TxtName.Text = dosageForm.Name;
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
                    bool duplicateExists = context.DosageForms
                        .AsEnumerable()
                        .Any(df => df.Id != _id.Value &&
                                  df.Name != null &&
                                  df.Name.Trim().ToLowerInvariant() == nameLower);
                    if (duplicateExists)
                    {
                        MessageBox.Show(
                            $"Форма дозировки «{name}» уже существует.",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    var dosageForm = context.DosageForms.Find(_id.Value);
                    if (dosageForm != null)
                    {
                        dosageForm.Name = name;
                        context.SaveChanges();
                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    bool exists = context.DosageForms
                        .AsEnumerable()
                        .Any(df => df.Name != null &&
                                  df.Name.Trim().ToLowerInvariant() == nameLower);
                    if (exists)
                    {
                        MessageBox.Show(
                            $"Форма дозировки «{name}» уже существует.",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    context.DosageForms.Add(new DosageForm { Name = name });
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