using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace ClinicSoft.Views.Admin
{
    public partial class EditUnitOfMeasurementWindow : Window
    {
        private readonly int? _id;
        public EditUnitOfMeasurementWindow(int? id)
        {
            InitializeComponent();
            _id = id;
            if (_id.HasValue)
            {
                Title = "Редактирование единицы измерения";
                FormTitle.Text = "Редактирование единицы измерения";
                BtnSave.Content = "Сохранить";
                LoadData();
            }
            else
            {
                Title = "Добавление единицы измерения";
                FormTitle.Text = "Добавление новой единицы измерения";
                BtnSave.Content = "Добавить";
            }
        }
        private void LoadData()
        {
            using var context = new ClinicSoftContext();
            var unit = context.UnitsOfMeasurements.Find(_id.Value);
            if (unit != null)
            {
                TxtName.Text = unit.Symbol;
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
                    bool duplicateExists = context.UnitsOfMeasurements
                        .AsEnumerable()
                        .Any(u => u.Id != _id.Value &&
                                 u.Symbol != null &&
                                 u.Symbol.Trim().ToLowerInvariant() == nameLower);
                    if (duplicateExists)
                    {
                        MessageBox.Show("Такая единица измерения уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    var unit = context.UnitsOfMeasurements.Find(_id.Value);
                    if (unit != null)
                    {
                        unit.Symbol = name;
                        context.SaveChanges();
                        DialogResult = true;
                        Close();
                    }
                }
                else
                {
                    bool exists = context.UnitsOfMeasurements
                        .AsEnumerable()
                        .Any(u => u.Symbol != null &&
                                 u.Symbol.Trim().ToLowerInvariant() == nameLower);

                    if (exists)
                    {
                        MessageBox.Show("Такая единица измерения уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    context.UnitsOfMeasurements.Add(new UnitsOfMeasurement { Symbol = name });
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