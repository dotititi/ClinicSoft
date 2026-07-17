using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClinicSoft.Views.Admin.AddWindow
{
    public partial class AddPatientWindow : Window
    {
        private readonly Brush _defaultBorderBrush = new SolidColorBrush(Color.FromRgb(189, 189, 189));
        private readonly Brush _errorBorderBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        private readonly Brush _successBorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));
        private bool _isBirthdayValid = false;
        private bool _isInitialLoad = true;
        public AddPatientWindow()
        {
            InitializeComponent();
            SetupFieldStyles();
            CbGender.SelectedIndex = 0;
            DpBirthday.SelectedDate = DateTime.Today.AddYears(-30);
            SetFormattedPhone("+7 ");
            _isInitialLoad = false;
            TxtLastName.TextChanged += (s, e) => ValidateNameField(TxtLastName);
            TxtFirstName.TextChanged += (s, e) => ValidateNameField(TxtFirstName);
            TxtMiddleName.TextChanged += (s, e) => ValidateNameField(TxtMiddleName);
            TxtEmail.TextChanged += (s, e) => ValidateEmailField();
            TxtInsurance.TextChanged += (s, e) => ValidateInsuranceField();
            TxtAllergies.TextChanged += (s, e) => ValidateRequiredField(TxtAllergies);
            TxtChronic.TextChanged += (s, e) => ValidateRequiredField(TxtChronic);
            DpBirthday.SelectedDateChanged += (s, e) => ValidateBirthdayField();
        }
        private void SetupFieldStyles()
        {
            foreach (var control in new Control[] {
                TxtLastName, TxtFirstName, TxtMiddleName, TxtPhone,
                TxtEmail, TxtInsurance, TxtAllergies, TxtChronic
            })
            {
                if (control is TextBox tb)
                {
                    tb.BorderBrush = _defaultBorderBrush;
                    tb.BorderThickness = new Thickness(1);
                }
            }
            DpBirthday.Background = Brushes.White;
        }
        private void SetFormattedPhone(string phone)
        {
            TxtPhone.TextChanged -= TxtPhone_TextChanged;
            try
            {
                TxtPhone.Text = FormatPhoneDisplay(phone);
                TxtPhone.SelectionStart = TxtPhone.Text.Length;
                if (!_isInitialLoad)
                    ValidatePhoneField();
            }
            finally
            {
                TxtPhone.TextChanged += TxtPhone_TextChanged;
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateNameField(TxtLastName))
            {
                ShowFieldError(TxtLastName, "Поле 'Фамилия' обязательно для заполнения.");
                return;
            }
            if (!ValidateNameField(TxtFirstName))
            {
                ShowFieldError(TxtFirstName, "Поле 'Имя' обязательно для заполнения.");
                return;
            }
            if (!ValidateBirthdayField())
            {
                ShowFieldError(DpBirthday, "Укажите корректную дату рождения.");
                return;
            }
            if (CbGender.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите пол.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidatePhoneField())
            {
                ShowFieldError(TxtPhone, "Телефон должен содержать ровно 10 цифр после +7");
                return;
            }
            if (!IsPhoneUnique(TxtPhone.Text))
            {
                ShowFieldError(TxtPhone, "Пациент с таким номером телефона уже существует в системе.");
                return;
            }
            string emailInput = TxtEmail.Text.Trim();
            if (string.IsNullOrWhiteSpace(emailInput))
            {
                ShowFieldError(TxtEmail, "Email обязателен для заполнения.");
                return;
            }
            if (!IsValidEmail(emailInput))
            {
                ShowFieldError(TxtEmail, "Введите корректный email (пример: user@example.com)");
                return;
            }
            if (!IsEmailUnique(emailInput))
            {
                ShowFieldError(TxtEmail, "Пользователь с таким email уже существует в системе.");
                return;
            }
            string insuranceNumber = TxtInsurance.Text.Trim();
            if (!ValidateInsuranceField())
            {
                ShowFieldError(TxtInsurance, "Номер полиса должен содержать ровно 16 цифр.");
                return;
            }
            if (!IsInsuranceUnique(insuranceNumber))
            {
                ShowFieldError(TxtInsurance, "Пациент с таким номером полиса уже зарегистрирован.");
                return;
            }
            if (!ValidateRequiredField(TxtAllergies))
            {
                ShowFieldError(TxtAllergies, "Поле 'Аллергии' обязательно для заполнения.");
                return;
            }
            if (!ValidateRequiredField(TxtChronic))
            {
                ShowFieldError(TxtChronic, "Поле 'Хронические заболевания' обязательно для заполнения.");
                return;
            }
            try
            {
                int genderId = CbGender.SelectedIndex == 1 ? 2 : 1;
                string lastName = TxtLastName.Text.Trim();
                string firstName = TxtFirstName.Text.Trim();
                DateTime birthDate = DpBirthday.SelectedDate.Value.Date;
                string passwordHash = BCrypt.Net.BCrypt.HashPassword("patient123");
                using var context = new ClinicSoftContext();
                var patientRole = context.Roles.FirstOrDefault(r => r.Name == "patient");
                if (patientRole == null)
                {
                    MessageBox.Show("Роль 'patient' не найдена в системе. Обратитесь к администратору.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var user = new User
                {
                    Login = emailInput.Split('@')[0].ToLowerInvariant(),
                    PasswordHash = passwordHash,
                    RoleId = patientRole.Id
                };
                context.Users.Add(user);
                context.SaveChanges();
                var patient = new Patient
                {
                    UserId = user.Id,
                    LastName = lastName,
                    FirstName = firstName,
                    MiddleName = TxtMiddleName.Text.Trim(),
                    Birthday = DateOnly.FromDateTime(birthDate),
                    GenderCode = genderId,
                    Phone = FormatPhoneForSave(TxtPhone.Text),
                    Email = emailInput
                };
                context.Patients.Add(patient);
                context.SaveChanges();
                var card = new MedicalCard
                {
                    PatientId = patient.Id,
                    InsuranceNumber = insuranceNumber,
                    Allergies = TxtAllergies.Text.Trim(),
                    ChronicConditions = TxtChronic.Text.Trim()
                };
                context.MedicalCards.Add(card);
                context.SaveChanges();
                MessageBox.Show(
                    $"Пациент успешно добавлен!\nЛогин: {user.Login}\nПароль: patient123",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Не удалось сохранить пациента:\n\n{errorMessage}",
                               "Критическая ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
        private void DpBirthday_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateBirthdayField();
        }
        private bool ValidateBirthdayField()
        {
            bool isValid = DpBirthday.SelectedDate != null &&
                          DpBirthday.SelectedDate.Value.Date <= DateTime.Today.Date;
            _isBirthdayValid = isValid;
            if (!isValid)
            {
                DpBirthday.Background = new SolidColorBrush(Color.FromRgb(255, 245, 245));
            }
            else
            {
                DpBirthday.Background = Brushes.White;
            }

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
                return !context.Patients.Any(p => p.Phone == formatted);
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
        private void TxtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateEmailField();
        }
        private bool ValidateEmailField()
        {
            string email = TxtEmail.Text.Trim();
            bool isValid = !string.IsNullOrWhiteSpace(email) && IsValidEmail(email);
            TxtEmail.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            TxtEmail.BorderThickness = new Thickness(2);
            return isValid;
        }
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
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
                string login = email.Contains('@')
                    ? email.Split('@')[0].ToLowerInvariant()
                    : email.ToLowerInvariant();
                using var context = new ClinicSoftContext();
                return !context.Patients.Any(p => p.Email == email) &&
                       !context.Users.Any(u => u.Login == login);
            }
            catch
            {
                return false;
            }
        }
        private void TxtInsurance_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
        private void TxtInsurance_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                e.Handled = true;
        }
        private void TxtInsurance_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string digitsOnly = new string(textBox.Text.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length > 16)
                digitsOnly = digitsOnly.Substring(0, 16);

            if (textBox.Text != digitsOnly)
            {
                int cursor = textBox.SelectionStart;
                textBox.Text = digitsOnly;
                textBox.SelectionStart = Math.Min(cursor, textBox.Text.Length);
            }

            ValidateInsuranceField();
        }
        private bool ValidateInsuranceField()
        {
            string digitsOnly = new string(TxtInsurance.Text.Where(char.IsDigit).ToArray());
            bool isValid = digitsOnly.Length == 16;
            TxtInsurance.BorderBrush = isValid ? _successBorderBrush :
                (string.IsNullOrEmpty(digitsOnly) ? _defaultBorderBrush : _errorBorderBrush);
            TxtInsurance.BorderThickness = new Thickness(2);
            return isValid;
        }
        private bool IsInsuranceUnique(string insuranceNumber)
        {
            try
            {
                using var context = new ClinicSoftContext();
                return !context.MedicalCards.Any(mc => mc.InsuranceNumber == insuranceNumber);
            }
            catch
            {
                return false;
            }
        }
        private bool ValidateNameField(TextBox tb)
        {
            bool isValid = !string.IsNullOrWhiteSpace(tb.Text);
            tb.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            tb.BorderThickness = new Thickness(2);
            return isValid;
        }
        private bool ValidateRequiredField(TextBox tb)
        {
            bool isValid = !string.IsNullOrWhiteSpace(tb.Text);
            tb.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            tb.BorderThickness = new Thickness(2);
            return isValid;
        }
        private void ShowFieldError(Control control, string message)
        {
            MessageBox.Show(message, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            control.Focus();
            if (control is TextBox tb)
            {
                tb.BorderBrush = _errorBorderBrush;
                tb.BorderThickness = new Thickness(2);
            }
            else if (control is DatePicker dp)
            {
                dp.Background = new SolidColorBrush(Color.FromRgb(255, 245, 245));
            }
        }
    }
}