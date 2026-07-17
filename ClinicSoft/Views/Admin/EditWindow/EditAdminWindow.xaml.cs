using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions; // 🔑 Добавьте в начало файла


namespace ClinicSoft.Views.Admin.EditWindow
{
    /// <summary>
    /// Логика взаимодействия для EditAdminWindow.xaml
    /// </summary>
    public partial class EditAdminWindow : Window
    {
        private readonly int _adminId;
        private readonly Brush _defaultBorderBrush = new SolidColorBrush(Color.FromRgb(189, 189, 189));
        private readonly Brush _errorBorderBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        private readonly Brush _successBorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));

        public EditAdminWindow(int adminId)
        {
            InitializeComponent();
            _adminId = adminId;
            TxtLastName.TextChanged += (s, e) => ValidateNameField(TxtLastName);
            TxtFirstName.TextChanged += (s, e) => ValidateNameField(TxtFirstName);
            TxtMiddleName.TextChanged += (s, e) => ValidateOptionalField(TxtMiddleName);
            TxtEmail.TextChanged += (s, e) => ValidateEmailField();
            DpBirthday.SelectedDateChanged += (s, e) => ValidateBirthdayField();
            TxtPhone.TextChanged += (s, e) => ValidatePhoneField();
            LoadAdminData();
        }
        private void LoadAdminData()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var admin = context.Admins
                    .Include(a => a.User)
                    .FirstOrDefault(a => a.Id == _adminId);
                if (admin == null)
                {
                    MessageBox.Show("Администратор не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }
                var genders = context.Genders.ToList();
                CbGender.ItemsSource = genders;
                TxtLastName.Text = admin.LastName ?? "";
                TxtFirstName.Text = admin.FirstName ?? "";
                TxtMiddleName.Text = admin.MiddleName ?? "";
                DpBirthday.SelectedDate = admin.Birthday.ToDateTime(TimeOnly.MinValue);
                TxtEmail.Text = admin.Email ?? "";
                SetFormattedPhone(admin.Phone ?? "+7 ");
                TxtLogin.Text = admin.User?.Login ?? "";
                CbGender.SelectedValue = admin.GenderCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtLastName.Text))
            {
                ShowError("Фамилия обязательна для заполнения.", TxtLastName);
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtFirstName.Text))
            {
                ShowError("Имя обязательно для заполнения.", TxtFirstName);
                return;
            }
            if (DpBirthday.SelectedDate == null || DpBirthday.SelectedDate.Value.Date > DateTime.Today.Date)
            {
                ShowError("Укажите корректную дату рождения (не из будущего).", DpBirthday);
                return;
            }
            string email = TxtEmail.Text.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Email обязателен для заполнения.", TxtEmail);
                return;
            }
            if (!IsValidEmail(email))
            {
                ShowError("Введите корректный email (например: petrov@clinic.ru).", TxtEmail);
                return;
            }
            if (!IsEmailUnique(email))
            {
                ShowError("Данный email уже используется другим пользователем.", TxtEmail);
                return;
            }
            if (!IsValidPhone(TxtPhone.Text))
            {
                ShowError("Телефон должен содержать ровно 10 цифр после +7", TxtPhone);
                return;
            }
            if (!IsPhoneUnique(TxtPhone.Text))
            {
                ShowError("Данный номер телефона уже используется другим пользователем.", TxtPhone);
                return;
            }
            if (CbGender.SelectedValue == null)
            {
                ShowError("Выберите пол.", CbGender);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                var admin = context.Admins
                    .Include(a => a.User)
                    .FirstOrDefault(a => a.Id == _adminId);
                if (admin == null)
                {
                    MessageBox.Show("Администратор не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                admin.LastName = TxtLastName.Text.Trim();
                admin.FirstName = TxtFirstName.Text.Trim();
                admin.MiddleName = string.IsNullOrWhiteSpace(TxtMiddleName.Text) ? null : TxtMiddleName.Text.Trim();
                admin.Email = email;
                admin.Phone = FormatPhoneForSave(TxtPhone.Text);
                admin.Birthday = DateOnly.FromDateTime(DpBirthday.SelectedDate.Value.Date);
                admin.GenderCode = (int)CbGender.SelectedValue;
                context.SaveChanges();
                MessageBox.Show("Данные администратора успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CbGender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateGenderField();
        }
        private bool ValidateGenderField()
        {
            bool isValid = CbGender.SelectedValue != null;
            CbGender.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            CbGender.BorderThickness = new Thickness(2);
            UpdateSaveButtonState();
            return isValid;
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void SetFormattedPhone(string phone)
        {
            TxtPhone.TextChanged -= TxtPhone_TextChanged;
            try
            {
                TxtPhone.Text = FormatPhoneDisplay(phone);
                TxtPhone.SelectionStart = TxtPhone.Text.Length;
            }
            finally
            {
                TxtPhone.TextChanged += TxtPhone_TextChanged;
            }
        }
        private string FormatPhoneDisplay(string phone)
        {
            string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length <= 1) return "+7 ";
            if (digitsOnly.Length > 11) digitsOnly = digitsOnly.Substring(0, 11);
            if (digitsOnly[0] == '8') digitsOnly = "7" + digitsOnly.Substring(1);
            if (digitsOnly.Length <= 1) return "+7 ";
            string result = "+7 ";
            string digitsAfter7 = digitsOnly.Substring(1);
            if (digitsAfter7.Length > 0)
            {
                result += digitsAfter7.Substring(0, Math.Min(3, digitsAfter7.Length));
                if (digitsAfter7.Length > 3)
                {
                    result += " " + digitsAfter7.Substring(3, Math.Min(3, digitsAfter7.Length - 3));
                    if (digitsAfter7.Length > 6)
                    {
                        result += "-" + digitsAfter7.Substring(6, Math.Min(2, digitsAfter7.Length - 6));
                        if (digitsAfter7.Length > 8)
                        {
                            result += "-" + digitsAfter7.Substring(8);
                        }
                    }
                }
            }
            return result;
        }
        private void TxtPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
        private void TxtPhone_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                e.Handled = true;
        }
        private void TxtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            textBox.TextChanged -= TxtPhone_TextChanged;
            try
            {
                string originalText = textBox.Text;
                if (string.IsNullOrEmpty(originalText) || !originalText.StartsWith("+7"))
                {
                    textBox.Text = "+7 ";
                    textBox.SelectionStart = 4;
                    ValidatePhoneField(false);
                    return;
                }
                string digitsOnly = new string(originalText.Where(char.IsDigit).ToArray());
                if (digitsOnly.Length > 0 && digitsOnly[0] == '8')
                    digitsOnly = "7" + digitsOnly.Substring(1);
                if (digitsOnly.Length > 11)
                    digitsOnly = digitsOnly.Substring(0, 11);
                if (digitsOnly.Length <= 1)
                {
                    textBox.Text = "+7 ";
                    textBox.SelectionStart = 4;
                    ValidatePhoneField(false);
                    return;
                }
                string formatted = FormatPhoneDisplay("+7" + digitsOnly.Substring(1));
                textBox.Text = formatted;
                textBox.SelectionStart = textBox.Text.Length;
                ValidatePhoneField(IsValidPhone(textBox.Text));
            }
            catch (Exception ex)
            {
                textBox.Text = "+7 ";
                textBox.SelectionStart = 4;
                ValidatePhoneField(false);
                System.Diagnostics.Debug.WriteLine($"Ошибка форматирования телефона: {ex.Message}");
            }
            finally
            {
                textBox.TextChanged += TxtPhone_TextChanged;
            }
        }
        private bool IsValidPhone(string phone)
        {
            var digitsOnly = Regex.Replace(phone, @"[^0-9]", "");
            return digitsOnly.Length == 11 && digitsOnly.StartsWith("7");
        }
        private bool IsPhoneUnique(string phone)
        {
            try
            {
                using var context = new ClinicSoftContext();
                string formatted = FormatPhoneForSave(phone);
                return !context.Admins.Any(a => a.Id != _adminId && a.Phone == formatted) &&
                       !context.Doctors.Any(d => d.Phone == formatted) &&
                       !context.Patients.Any(p => p.Phone == formatted) &&
                       !context.Registrators.Any(r => r.Phone == formatted);
            }
            catch
            {
                return false;
            }
        }
        private string FormatPhoneForSave(string phone)
        {
            var digitsOnly = Regex.Replace(phone, @"[^0-9]", "");
            return digitsOnly.Length == 11 ? $"+{digitsOnly}" : phone;
        }
        private bool ValidatePhoneField(bool? isValid = null)
        {
            if (isValid == null)
                isValid = IsValidPhone(TxtPhone.Text);
            TxtPhone.BorderBrush = isValid.Value ? _successBorderBrush : _errorBorderBrush;
            TxtPhone.BorderThickness = new Thickness(2);
            UpdateSaveButtonState();
            return isValid.Value;
        }
        private bool ValidateNameField(TextBox tb)
        {
            bool isValid = !string.IsNullOrWhiteSpace(tb.Text);
            tb.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            tb.BorderThickness = new Thickness(2);
            UpdateSaveButtonState();
            return isValid;
        }
        private void ValidateOptionalField(TextBox tb)
        {
            tb.BorderBrush = _defaultBorderBrush;
            tb.BorderThickness = new Thickness(1);
            UpdateSaveButtonState();
        }
        private bool ValidateEmailField()
        {
            string email = TxtEmail.Text.Trim();
            bool isValid = !string.IsNullOrWhiteSpace(email) && IsValidEmail(email);
            TxtEmail.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            TxtEmail.BorderThickness = new Thickness(2);
            UpdateSaveButtonState();
            return isValid;
        }
        private bool ValidateBirthdayField()
        {
            bool isValid = DpBirthday.SelectedDate != null &&
                          DpBirthday.SelectedDate.Value.Date <= DateTime.Today.Date;
            if (!isValid)
            {
                DpBirthday.Background = new SolidColorBrush(Color.FromRgb(255, 245, 245));
            }
            else
            {
                DpBirthday.Background = Brushes.White;
            }
            UpdateSaveButtonState();
            return isValid;
        }
        private void UpdateSaveButtonState()
        {
            bool isValid =
                !string.IsNullOrWhiteSpace(TxtLastName.Text) &&
                !string.IsNullOrWhiteSpace(TxtFirstName.Text) &&
                DpBirthday.SelectedDate != null &&
                DpBirthday.SelectedDate.Value.Date <= DateTime.Today.Date &&
                !string.IsNullOrWhiteSpace(TxtEmail.Text) &&
                IsValidEmail(TxtEmail.Text.Trim()) &&
                IsValidPhone(TxtPhone.Text) &&
                CbGender.SelectedValue != null;
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.Contains("@") && email.Contains(".");
            }
            catch
            {
                return false;
            }
        }
        private bool IsEmailUnique(string email)
        {
            try
            {
                using var context = new ClinicSoftContext();
                return !context.Admins.Any(a => a.Id != _adminId && a.Email == email) &&
                       !context.Doctors.Any(d => d.Email == email) &&
                       !context.Patients.Any(p => p.Email == email) &&
                       !context.Registrators.Any(r => r.Email == email);
            }
            catch
            {
                return false;
            }
        }
        private void ShowError(string message, Control control)
        {
            MessageBox.Show(message, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            control.Focus();
        }
    }
}
