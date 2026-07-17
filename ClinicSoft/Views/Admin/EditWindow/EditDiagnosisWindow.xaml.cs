using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace ClinicSoft.Views.Admin
{
    public partial class EditDiagnosisWindow : Window
    {
        private readonly int? _id;

        public EditDiagnosisWindow(int? id)
        {
            InitializeComponent();
            _id = id;
            if (_id.HasValue)
            {
                Title = "Редактировать диагноз";
                FormTitle.Text = "Редактирование диагноза";
                BtnSave.Content = "Сохранить";
                LoadData();
            }
            else
            {
                Title = "Добавление диагноза";
                FormTitle.Text = "Добавление нового диагноза";
                BtnSave.Content = "Добавить";
            }
        }
        private void LoadData()
        {
            using var context = new ClinicSoftContext();
            var diagnosis = context.Diagnoses.Find(_id.Value);
            if (diagnosis != null)
            {
                TxtName.Text = diagnosis.Name ?? "";
                TxtDescription.Text = diagnosis.Description ?? "";
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var name = TxtName.Text?.Trim();
            var description = TxtDescription.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Название диагноза не может быть пустым.", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Описание диагноза не может быть пустым.", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                if (_id.HasValue)
                {
                    var diagnosis = context.Diagnoses.Find(_id.Value);
                    if (diagnosis != null)
                    {
                        string nameLower = name.ToLowerInvariant();
                        bool duplicateExists = context.Diagnoses
                            .AsEnumerable()
                            .Any(d => d.Id != _id.Value &&
                                     d.Name != null &&
                                     d.Name.Trim().ToLowerInvariant() == nameLower);
                        if (duplicateExists)
                        {
                            MessageBox.Show("Диагноз с таким названием уже существует.", "Ошибка",
                                           MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        diagnosis.Name = name;
                        diagnosis.Description = description;
                        context.SaveChanges();
                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    string nameLower = name.ToLowerInvariant();
                    var existing = context.Diagnoses
                        .AsEnumerable()
                        .FirstOrDefault(d => d.Name != null &&
                                           d.Name.Trim().ToLowerInvariant() == nameLower);
                    if (existing != null)
                    {
                        MessageBox.Show("Диагноз с таким названием уже существует.", "Ошибка",
                                       MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    context.Diagnoses.Add(new Diagnosis
                    {
                        Name = name,
                        Description = description
                    });
                    context.SaveChanges();
                    DialogResult = true;
                    Close();
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique") == true)
            {
                MessageBox.Show("Диагноз с таким названием уже существует.", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.InnerException?.Message ?? ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}