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
    public partial class AdminAddDoctorWindow : Window
    {
        private readonly Brush _defaultBorderBrush = new SolidColorBrush(Color.FromRgb(189, 189, 189));
        private readonly Brush _errorBorderBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        private readonly Brush _successBorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));
        private bool _isBirthdayValid = false;
        private bool _isInitialLoad = true; 
        public AdminAddDoctorWindow()
        {
            InitializeComponent();
            LoadLookupData();
            DpBirthday.SelectedDate = DateTime.Today.AddYears(-35);
            TxtPhone.Text = "+7 ";
            TxtLastName.TextChanged += (s, e) => { if (!_isInitialLoad) ValidateNameField(TxtLastName); };
            TxtFirstName.TextChanged += (s, e) => { if (!_isInitialLoad) ValidateNameField(TxtFirstName); };
            TxtMiddleName.TextChanged += (s, e) => { if (!_isInitialLoad) ValidateOptionalField(TxtMiddleName); };
            TxtEmail.TextChanged += (s, e) => { if (!_isInitialLoad) ValidateEmailField(); };
            DpBirthday.SelectedDateChanged += (s, e) => { if (!_isInitialLoad) ValidateBirthdayField(); };
            CbGender.SelectionChanged += (s, e) => { if (!_isInitialLoad) ValidateGenderField(); };
            _isInitialLoad = false;
        }
        private void LoadLookupData()
        {
            using var context = new ClinicSoftContext();
            var specialities = context.MedicalSpecialities.Select(s => new { s.Id, s.Name }).ToList();
            CbSpeciality.ItemsSource = specialities;
            CbSpeciality.DisplayMemberPath = "Name";
            CbSpeciality.SelectedValuePath = "Id";
            var departments = context.Departments.Select(d => new { d.Id, d.Name }).ToList();
            CbDepartment.ItemsSource = departments;
            CbDepartment.DisplayMemberPath = "Name";
            CbDepartment.SelectedValuePath = "Id";
            var offices = context.Offices.Select(o => new { o.Id, o.Number }).ToList();
            CbOffice.ItemsSource = offices;
            CbOffice.DisplayMemberPath = "Number";
            CbOffice.SelectedValuePath = "Id";
            var statuses = context.DoctorStatuses.Select(s => new { s.Id, s.Name }).ToList();
            CbStatus.ItemsSource = statuses;
            CbStatus.DisplayMemberPath = "Name";
            CbStatus.SelectedValuePath = "Id";
            if (statuses.Any())
                CbStatus.SelectedIndex = 0;
            var genders = context.Genders.ToList();
            CbGender.ItemsSource = genders;
            CbGender.DisplayMemberPath = "Name";
            CbGender.SelectedValuePath = "Id";
            if (genders.Any())
                CbGender.SelectedIndex = 0;
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var selectedOfficeId = CbOffice.SelectedValue as int?;
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
                ShowError("Введите корректный email (например: ivanov@clinic.ru).", TxtEmail);
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
                MessageBox.Show("Выберите пол.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CbSpeciality.SelectedValue == null)
            {
                MessageBox.Show("Выберите специальность.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CbDepartment.SelectedValue == null)
            {
                MessageBox.Show("Выберите отделение.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!selectedOfficeId.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите кабинет.");
                return;
            }
            if (CbStatus.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                string lastName = TxtLastName.Text.Trim();
                string firstName = TxtFirstName.Text.Trim();
                string middleName = TxtMiddleName.Text.Trim();
                int specialityId = (int)CbSpeciality.SelectedValue;
                string emailPrefix = email.Split('@')[0].ToLowerInvariant();
                string loginBase = $"doctor_{CleanLoginPart(emailPrefix)}";
                string login = GenerateUniqueLogin(loginBase);
                string defaultPassword = "doctor123";
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
                using var contextSave = new ClinicSoftContext();
                var doctorRole = contextSave.Roles.FirstOrDefault(r => r.Name == "doctor");
                if (doctorRole == null)
                {
                    MessageBox.Show("Роль 'doctor' не найдена в системе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var user = new User
                {
                    Login = login,
                    PasswordHash = passwordHash,
                    RoleId = doctorRole.Id
                };
                contextSave.Users.Add(user);
                contextSave.SaveChanges();
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    LastName = lastName,
                    FirstName = firstName,
                    MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName,
                    Birthday = DateOnly.FromDateTime(DpBirthday.SelectedDate.Value.Date),
                    Email = email,
                    Phone = FormatPhoneForSave(TxtPhone.Text),
                    GenderCode = (int)CbGender.SelectedValue,
                    SpecialityId = specialityId,
                    DepartmentId = (int)CbDepartment.SelectedValue,
                    OfficeId = selectedOfficeId.Value,
                    StatusId = (int)CbStatus.SelectedValue,
                };
                contextSave.Doctors.Add(doctor);
                contextSave.SaveChanges();
                MessageBox.Show(
                    $"Врач успешно добавлен!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
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
                return !context.Doctors.Any(d => d.Phone == formatted) &&
                       !context.Patients.Any(p => p.Phone == formatted) &&
                       !context.Admins.Any(a => a.Phone == formatted) &&
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
        private void TxtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isInitialLoad)
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
                using var context = new ClinicSoftContext();
                return !context.Doctors.Any(d => d.Email == email) &&
                       !context.Patients.Any(p => p.Email == email) &&
                       !context.Admins.Any(a => a.Email == email) &&
                       !context.Registrators.Any(r => r.Email == email);
            }
            catch
            {
                return false;
            }
        }
        private void DpBirthday_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialLoad)
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
        private string CleanLoginPart(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "user";

            return Regex.Replace(input, @"[^a-zA-Z0-9._-]", "");
        }
        private string GenerateUniqueLogin(string baseLogin)
        {
            if (baseLogin.Length > 45)
                baseLogin = baseLogin.Substring(0, 45);

            using var context = new ClinicSoftContext();
            string finalLogin = baseLogin;
            int counter = 1;
            while (context.Users.Any(u => u.Login == finalLogin))
            {
                string suffix = $"_{counter}";
                int maxLength = 50;
                int baseLength = maxLength - suffix.Length;
                if (baseLogin.Length > baseLength)
                    finalLogin = baseLogin.Substring(0, baseLength) + suffix;
                else
                    finalLogin = baseLogin + suffix;
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