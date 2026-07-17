using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.Admin
{
    public partial class AdminAdminPage : Page
    {
        private bool _isWrapped = false;
        private const double MIN_WIDTH_FOR_SINGLE_LINE = 680;
        public AdminAdminPage()
        {
            InitializeComponent();
            SyncSearchBoxes();
            LoadAdmins();
        }
        private void LoadAdmins(string search = null)
        {
            using var context = new ClinicSoftContext();
            var allAdmins = context.Admins
                .Include(a => a.User)
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .ToList();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.Trim().ToLowerInvariant();
                allAdmins = allAdmins
                    .Where(a =>
                        (!string.IsNullOrEmpty(a.LastName) && a.LastName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(a.FirstName) && a.FirstName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(a.MiddleName) && a.MiddleName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(a.Email) && a.Email.ToLowerInvariant().Contains(searchLower)) ||
                        (a.User?.Login != null && a.User.Login.ToLowerInvariant().Contains(searchLower)))
                    .ToList();
            }
            AdminDataGrid.ItemsSource = allAdmins;
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
            SyncSearchBoxes();
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                bool isEmpty = string.IsNullOrWhiteSpace(textBox.Text);
                if (textBox == SearchBox)
                {
                    SearchPlaceholder.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (textBox == SearchBox2)
                {
                    SearchPlaceholder2.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
                }
                SyncSearchBoxes();
                LoadAdmins(textBox.Text);
            }
        }
        private void SyncSearchBoxes()
        {
            if (SearchBox != null && SearchBox2 != null)
            {
                string currentText = "";
                if (SingleLineToolbar.Visibility == Visibility.Visible)
                {
                    currentText = SearchBox.Text ?? "";
                }
                else
                {
                    currentText = SearchBox2.Text ?? "";
                }
                SearchBox.Text = currentText;
                SearchBox2.Text = currentText;
                SearchPlaceholder.Visibility = string.IsNullOrWhiteSpace(currentText) ? Visibility.Visible : Visibility.Collapsed;
                SearchPlaceholder2.Visibility = string.IsNullOrWhiteSpace(currentText) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void AdminDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = AdminDataGrid.SelectedItem != null;
            BtnEdit.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
            BtnEdit2.IsEnabled = hasSelection;
            BtnDelete2.IsEnabled = hasSelection;
        }
        private void BtnAddAdmin_Click(object sender, RoutedEventArgs e)
        {
            var window = new Views.Admin.AddWindow.AddAdminWindow();
            if (window.ShowDialog() == true)
            {
                LoadAdmins();
            }
        }
        private void BtnEditAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (AdminDataGrid.SelectedItem is not ClinicSoft.Models.Admin selected)
            {
                MessageBox.Show("Выберите администратора для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var editWindow = new Views.Admin.EditWindow.EditAdminWindow(selected.Id);
            if (editWindow.ShowDialog() == true)
            {
                LoadAdmins();
            }
        }
        private void BtnDeleteAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (AdminDataGrid.SelectedItem is not ClinicSoft.Models.Admin selected)
            {
                MessageBox.Show("Выберите администратора для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string fullName = $"{selected.LastName} {selected.FirstName} {selected.MiddleName}".Trim();
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить администратора:\n{fullName}?\n\n" +
                "Внимание: Будут удалены все данные пользователя, включая учётную запись.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new ClinicSoftContext();
                    var admin = context.Admins
                        .Include(a => a.User)
                        .FirstOrDefault(a => a.Id == selected.Id);
                    if (admin == null)
                    {
                        MessageBox.Show("Администратор не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    int adminCount = context.Admins.Count();
                    if (adminCount == 1)
                    {
                        MessageBox.Show(
                            "Нельзя удалить последнего администратора системы.\n" +
                            "Сначала создайте нового администратора.",
                            "Удаление невозможно",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                    context.Users.Remove(admin.User);
                    context.SaveChanges();
                    MessageBox.Show(
                        $"Администратор \"{fullName}\" успешно удалён.",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    LoadAdmins();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка при удалении администратора:\n{ex.Message}",
                        "Критическая ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}