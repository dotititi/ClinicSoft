using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminRegistratorPage : Page
    {
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 680;
        private string _currentSearchText = "";
        public AdminRegistratorPage()
        {
            InitializeComponent();
            LoadRegistrators();
        }
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth < MIN_WIDTH_FOR_SINGLE_LINE)
            {
                SingleLineToolbar.Visibility = Visibility.Collapsed;
                MultiLineToolbar.Visibility = Visibility.Visible;
            }
            else
            {
                SingleLineToolbar.Visibility = Visibility.Visible;
                MultiLineToolbar.Visibility = Visibility.Collapsed;
            }
            UpdateSearchDisplay();
        }
        private void LoadRegistrators(string search = null)
        {
            using var context = new ClinicSoftContext();
            var allRegistrators = context.Registrators
                .Include(r => r.User)
                .OrderBy(r => r.LastName)
                .ThenBy(r => r.FirstName)
                .ToList();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.Trim().ToLowerInvariant();
                allRegistrators = allRegistrators
                    .Where(r =>
                        (!string.IsNullOrEmpty(r.LastName) && r.LastName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(r.FirstName) && r.FirstName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(r.MiddleName) && r.MiddleName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(r.Email) && r.Email.ToLowerInvariant().Contains(searchLower)) ||
                        (r.User?.Login != null && r.User.Login.ToLowerInvariant().Contains(searchLower)))
                    .ToList();
            }
            RegistratorDataGrid.ItemsSource = allRegistrators;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                _currentSearchText = textBox.Text ?? "";
                UpdateSearchDisplay();
                LoadRegistrators(_currentSearchText);
            }
        }
        private void UpdateSearchDisplay()
        {
            SearchBox.Text = _currentSearchText;
            SearchBox2.Text = _currentSearchText;
            SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
            SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(_currentSearchText) ? Visibility.Visible : Visibility.Collapsed;
        }
        private void RegistratorDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = RegistratorDataGrid.SelectedItem is Models.Registrator;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAddRegistrator_Click(object sender, RoutedEventArgs e)
        {
            var window = new Views.Admin.AddWindow.AddRegistratorWindow();
            if (window.ShowDialog() == true)
            {
                LoadRegistrators(_currentSearchText);
            }
        }
        private void BtnEditRegistrator_Click(object sender, RoutedEventArgs e)
        {
            if (RegistratorDataGrid.SelectedItem is not Models.Registrator selected)
            {
                MessageBox.Show("Выберите регистратора для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var editWindow = new Views.Admin.EditWindow.EditRegistratorWindow(selected.Id);
            if (editWindow.ShowDialog() == true)
            {
                LoadRegistrators(_currentSearchText);
            }
        }
        private void BtnDeleteRegistrator_Click(object sender, RoutedEventArgs e)
        {
            if (RegistratorDataGrid.SelectedItem is not Models.Registrator selected)
            {
                MessageBox.Show("Выберите регистратора для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string fullName = $"{selected.LastName} {selected.FirstName} {selected.MiddleName}".Trim();
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить регистратора:\n{fullName}?\n\n" +
                "Внимание: Будут удалены все данные пользователя, включая учётную запись.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new ClinicSoftContext();
                    var registrator = context.Registrators
                        .Include(r => r.User)
                        .FirstOrDefault(r => r.Id == selected.Id);
                    if (registrator == null)
                    {
                        MessageBox.Show("Регистратор не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    context.Users.Remove(registrator.User);
                    context.SaveChanges();
                    MessageBox.Show(
                        $"Регистратор \"{fullName}\" успешно удалён.",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    LoadRegistrators(_currentSearchText);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка при удалении регистратора:\n{ex.Message}",
                        "Критическая ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}