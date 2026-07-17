using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace ClinicSoft.Views.Admin.AddWindow
{
    public partial class AdminAddLabTestTypeWindow : Window
    {
        public AdminAddLabTestTypeWindow()
        {
            InitializeComponent();
            LoadUnits();
        }
        private void LoadUnits()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var units = context.UnitsOfMeasurements.ToList();
                CbUnit.ItemsSource = units;
                if (units.Count > 0)
                {
                    CbUnit.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки единиц измерения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string testName = TxtName.Text?.Trim();
            if (string.IsNullOrWhiteSpace(testName))
            {
                MessageBox.Show("Введите название типа анализа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtNormalRange.Text))
            {
                MessageBox.Show("Укажите норму для анализа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CbUnit.SelectedItem == null)
            {
                MessageBox.Show("Выберите единицу измерения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                bool exists = context.LabTestTypes
                    .AsEnumerable()
                    .Any(t => t.Name != null &&
                             t.Name.Trim().Equals(testName, StringComparison.OrdinalIgnoreCase));
                if (exists)
                {
                    MessageBox.Show(
                        $"Тип анализа с названием «{testName}» уже существует.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                var selectedUnit = (UnitsOfMeasurement)CbUnit.SelectedItem;
                context.LabTestTypes.Add(new LabTestType
                {
                    Name = testName,
                    NormalRange = TxtNormalRange.Text.Trim(),
                    UnitId = selectedUnit.Id
                });
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