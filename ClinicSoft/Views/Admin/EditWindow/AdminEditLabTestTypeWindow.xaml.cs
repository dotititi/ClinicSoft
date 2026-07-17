using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace ClinicSoft.Views.Admin.EditWindow
{
    public partial class AdminEditLabTestTypeWindow : Window
    {
        private readonly int _labTestTypeId;
        public AdminEditLabTestTypeWindow(int id, string name)
        {
            InitializeComponent();
            _labTestTypeId = id;
            LoadData(name);
        }
        private void LoadData(string initialName)
        {
            try
            {
                using var context = new ClinicSoftContext();
                var units = context.UnitsOfMeasurements.ToList();
                CbUnit.ItemsSource = units;
                var labTestType = context.LabTestTypes
                    .Include(ltt => ltt.Unit)
                    .FirstOrDefault(ltt => ltt.Id == _labTestTypeId);
                if (labTestType != null)
                {
                    TxtName.Text = labTestType.Name;
                    TxtNormalRange.Text = labTestType.NormalRange ?? "";
                    var selectedUnit = units.FirstOrDefault(u => u.Id == labTestType.UnitId);
                    if (selectedUnit != null)
                    {
                        CbUnit.SelectedItem = selectedUnit;
                    }
                }
                else
                {
                    TxtName.Text = initialName;
                    if (units.Count > 0)
                    {
                        CbUnit.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    .Any(t => t.Id != _labTestTypeId &&
                             t.Name != null &&
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
                var labTestType = context.LabTestTypes.Find(_labTestTypeId);
                if (labTestType != null)
                {
                    var selectedUnit = (UnitsOfMeasurement)CbUnit.SelectedItem;
                    labTestType.Name = testName;
                    labTestType.NormalRange = TxtNormalRange.Text.Trim();
                    labTestType.UnitId = selectedUnit.Id;
                    context.SaveChanges();
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Тип анализа не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}