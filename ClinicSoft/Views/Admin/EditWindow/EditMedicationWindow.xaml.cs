using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace ClinicSoft.Views.Admin
{
    public partial class EditMedicationWindow : Window
    {
        private readonly int? _id;
        public EditMedicationWindow(int? id)
        {
            InitializeComponent();
            _id = id;
            if (_id.HasValue)
            {
                Title = "Редактирование препарата";
                FormTitle.Text = "Редактирование препарата";
                BtnSave.Content = "Сохранить";
                LoadDosageForms();
                LoadData();
            }
            else
            {
                Title = "Добавление препарата";
                FormTitle.Text = "Добавление нового препарата";
                BtnSave.Content = "Добавить";
                LoadDosageForms();
            }
        }
        private void LoadDosageForms()
        {
            using var context = new ClinicSoftContext();
            var dosageForms = context.DosageForms
                .OrderBy(df => df.Name)
                .ToList();
            CbDosageForm.ItemsSource = dosageForms;
        }
        private void LoadData()
        {
            using var context = new ClinicSoftContext();
            var medication = context.Medications
                .Include(m => m.DosageForm)
                .FirstOrDefault(m => m.Id == _id.Value);
            if (medication != null)
            {
                TxtName.Text = medication.Name;
                CbDosageForm.SelectedValue = medication.DosageFormId;
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var name = TxtName.Text?.Trim();
            var dosageFormId = CbDosageForm.SelectedValue as int?;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Название не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!dosageFormId.HasValue)
            {
                MessageBox.Show("Выберите форму дозировки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                string nameLower = name.ToLowerInvariant();
                if (_id.HasValue)
                {
                    bool duplicateExists = context.Medications
                        .AsEnumerable()
                        .Any(m => m.Id != _id.Value &&
                                 m.Name != null &&
                                 m.Name.Trim().ToLowerInvariant() == nameLower &&
                                 m.DosageFormId == dosageFormId.Value);
                    if (duplicateExists)
                    {
                        var formName = (CbDosageForm.SelectedItem as DosageForm)?.Name ?? "выбранная";
                        MessageBox.Show(
                            $"Лекарство «{name}» с формой дозировки «{formName}» уже существует.",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    var medication = context.Medications.Find(_id.Value);
                    if (medication != null)
                    {
                        medication.Name = name;
                        medication.DosageFormId = dosageFormId.Value;
                        context.SaveChanges();
                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    bool exists = context.Medications
                        .AsEnumerable()
                        .Any(m => m.Name != null &&
                                 m.Name.Trim().ToLowerInvariant() == nameLower &&
                                 m.DosageFormId == dosageFormId.Value);
                    if (exists)
                    {
                        var formName = (CbDosageForm.SelectedItem as DosageForm)?.Name ?? "выбранная";
                        MessageBox.Show(
                            $"Лекарство «{name}» с формой дозировки «{formName}» уже существует.",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                    context.Medications.Add(new Medication
                    {
                        Name = name,
                        DosageFormId = dosageFormId.Value
                    });
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