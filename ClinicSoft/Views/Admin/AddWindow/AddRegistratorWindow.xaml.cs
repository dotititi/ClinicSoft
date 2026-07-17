using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClinicSoft.Views.Admin.AddWindow
{
    public partial class AddRegistratorWindow : Window
    {
        private readonly Brush _defaultBorderBrush = new SolidColorBrush(Color.FromRgb(189, 189, 189));
        private readonly Brush _errorBorderBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        private readonly Brush _successBorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));
        private bool _isInitialLoad = true; 
        public AddRegistratorWindow()
        {
            InitializeComponent();
            DpBirthday.SelectedDate = DateTime.Today.AddYears(-35);
            TxtPhone.Text = "+7 "; 
            LoadGenders();
            TxtLastName.TextChanged += (s, e) => { if (!_isInitialLoad) ValidateNameField(TxtLastName); };
            TxtFirstName.TextChanged += (s, e) => { if (!_isInitialLoad) ValidateNameField(TxtFirstName); };
            TxtMiddleName.TextChanged += (s, e) => { if (!_isInitialLoad) ValidateOptionalField(TxtMiddleName); };
            TxtEmail.TextChanged += (s, e) => { if (!_isInitialLoad) ValidateEmailField(); };
            DpBirthday.SelectedDateChanged += (s, e) => { if (!_isInitialLoad) ValidateBirthdayField(); };
            CbGender.SelectionChanged += (s, e) => { if (!_isInitialLoad) ValidateGenderField(); };
            _isInitialLoad = false;
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
                ShowError("Укажите корректную дату рождения.", DpBirthday);
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
                ShowError("Введите корректный email (например: reg@clinic.ru).", TxtEmail);
                return;
            }
            if (!IsEmailUnique(email))
            {
                ShowError("Данный email уже используется в системе.", TxtEmail);
                return;
            }
            if (!IsValidPhone(TxtPhone.Text))
            {
                ShowError("Телефон должен содержать ровно 10 цифр после +7", TxtPhone);
                return;
            }
            if (!IsPhoneUnique(TxtPhone.Text))
            {
                ShowError("Данный номер телефона уже используется в системе.", TxtPhone);
                return;
            }
            if (CbGender.SelectedValue == null)
            {
                ShowError("Выберите пол.", CbGender);
                return;
            }
            string loginBase = email.Split('@')[0].ToLowerInvariant();
            string login = GenerateUniqueLogin(loginBase);
            try
            {
                using var context = new ClinicSoftContext();
                var registratorRole = context.Roles.FirstOrDefault(r => r.Name == "registrator");
                if (registratorRole == null)
                {
                    MessageBox.Show("Роль 'registrator' не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string password = "registrator123";
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                var user = new User
                {
                    Login = login,
                    PasswordHash = passwordHash,
                    RoleId = registratorRole.Id
                };
                context.Users.Add(user);
                context.SaveChanges();
                var registrator = new Models.Registrator
                {
                    UserId = user.Id,
                    LastName = TxtLastName.Text.Trim(),
                    FirstName = TxtFirstName.Text.Trim(),
                    MiddleName = string.IsNullOrWhiteSpace(TxtMiddleName.Text) ? null : TxtMiddleName.Text.Trim(),
                    Email = email,
                    Phone = FormatPhoneForSave(TxtPhone.Text),
                    Birthday = DateOnly.FromDateTime(DpBirthday.SelectedDate.Value.Date),
                    GenderCode = (int)CbGender.SelectedValue
                };
                context.Registrators.Add(registrator);
                context.SaveChanges();
                MessageBox.Show(
                    $"Регистратор успешно добавлен!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания регистратора:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void LoadGenders()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var genders = context.Genders.ToList();
                CbGender.ItemsSource = genders;
                if (genders.Count > 0)
                {
                    CbGender.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки полов:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool ValidateGenderField()
        {
            bool isValid = CbGender.SelectedValue != null;
            CbGender.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            CbGender.BorderThickness = new Thickness(2);
            return isValid;
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
            if (textBox == null || _isInitialLoad) return;
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
        private string FormatPhoneForSave(string phone)
        {
            var digitsOnly = Regex.Replace(phone, @"[^0-9]", "");
            return digitsOnly.Length == 11 ? $"+{digitsOnly}" : phone;
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
                return !context.Admins.Any(a => a.Phone == formatted) &&
                       !context.Doctors.Any(d => d.Phone == formatted) &&
                       !context.Patients.Any(p => p.Phone == formatted) &&
                       !context.Registrators.Any(r => r.Phone == formatted);
            }
            catch
            {
                return false;
            }
        }
        private bool ValidatePhoneField(bool? isValid = null)
        {
            if (isValid == null)
                isValid = IsValidPhone(TxtPhone.Text);
            TxtPhone.BorderBrush = isValid.Value ? _successBorderBrush : _errorBorderBrush;
            TxtPhone.BorderThickness = new Thickness(2);
            return isValid.Value;
        }
        private bool ValidateNameField(TextBox tb)
        {
            bool isValid = !string.IsNullOrWhiteSpace(tb.Text);
            tb.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            tb.BorderThickness = new Thickness(2);
            return isValid;
        }
        private void ValidateOptionalField(TextBox tb)
        {
            tb.BorderBrush = _defaultBorderBrush;
            tb.BorderThickness = new Thickness(1);
        }
        private bool ValidateEmailField()
        {
            string email = TxtEmail.Text.Trim();
            bool isValid = !string.IsNullOrWhiteSpace(email) && IsValidEmail(email);
            TxtEmail.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            TxtEmail.BorderThickness = new Thickness(2);
            return isValid;
        }
        private bool ValidateBirthdayField()
        {
            bool isValid = DpBirthday.SelectedDate != null &&
                          DpBirthday.SelectedDate.Value.Date <= DateTime.Today.Date;
            DpBirthday.Background = isValid ? Brushes.White : new SolidColorBrush(Color.FromRgb(255, 245, 245));
            return isValid;
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
                return !context.Admins.Any(a => a.Email == email) &&
                       !context.Doctors.Any(d => d.Email == email) &&
                       !context.Patients.Any(p => p.Email == email) &&
                       !context.Registrators.Any(r => r.Email == email);
            }
            catch
            {
                return false;
            }
        }
        private string GenerateUniqueLogin(string basePart)
        {
            string cleanPart = Regex.Replace(basePart, @"[^a-zA-Z0-9._-]", "");
            if (string.IsNullOrWhiteSpace(cleanPart))
                cleanPart = "registrator_";
            string baseLogin = $"registrator_{cleanPart}";
            using var context = new ClinicSoftContext();
            string finalLogin = baseLogin;
            int counter = 1;
            while (context.Users.Any(u => u.Login == finalLogin))
            {
                finalLogin = $"{baseLogin}_{counter}";
                counter++;
                if (counter > 1000)
                    throw new Exception("Не удалось сгенерировать уникальный логин");
            }
            return finalLogin;
        }
        private void ShowError(string message, Control control)
        {
            MessageBox.Show(message, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            control.Focus();
        }
    }
}