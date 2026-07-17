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

namespace ClinicSoft.Views.Admin
{
    public partial class AdminProfileSettingsWindow : Window
    {
        private readonly int _userId;
        private readonly bool _isInitialSetup;
        private bool _isDataLoaded = false;
        private bool _isBirthdayValid = false;
        private readonly Brush _defaultBorderBrush = new SolidColorBrush(Color.FromRgb(189, 189, 189));
        private readonly Brush _errorBorderBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        private readonly Brush _successBorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));
        public AdminProfileSettingsWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            TxtLastName.TextChanged += (s, e) => ValidateNameField(TxtLastName);
            TxtFirstName.TextChanged += (s, e) => ValidateNameField(TxtFirstName);
            TxtMiddleName.TextChanged += (s, e) => ValidateOptionalField(TxtMiddleName);
            TxtEmail.TextChanged += (s, e) => ValidateEmailField();
            DpBirthday.SelectedDateChanged += (s, e) => ValidateBirthdayField();
            try
            {
                _isInitialSetup = !LoadProfileData();
                SetupUiForMode();
                _isDataLoaded = true;
                ValidateForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке профиля:\n{ex.Message}\n\nОкно открыто в режиме первого заполнения.",
                    "Ошибка загрузки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                _isInitialSetup = true;
                SetupUiForMode();
                _isDataLoaded = true;
                DpBirthday.SelectedDate = DateTime.Today.AddYears(-35);
                SetFormattedPhone("+7 ");
                using var context = new ClinicSoftContext();
                var defaultGender = context.Genders.FirstOrDefault();
                TxtGender.Text = defaultGender?.Name ?? "Не указан";
            }
        }
        private bool LoadProfileData()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var admin = context.Admins
                    .Include(a => a.User)
                    .FirstOrDefault(a => a.UserId == _userId);
                if (admin == null) return false;
                TxtLastName.Text = admin.LastName ?? "";
                TxtFirstName.Text = admin.FirstName ?? "";
                TxtMiddleName.Text = admin.MiddleName ?? "";
                DpBirthday.SelectedDate = admin.Birthday.ToDateTime(TimeOnly.MinValue);
                TxtEmail.Text = admin.Email ?? "";
                SetFormattedPhone(admin.Phone ?? "+7 ");
                TxtLogin.Text = admin.User?.Login ?? "";
                var gender = context.Genders.FirstOrDefault(g => g.Id == admin.GenderCode);
                TxtGender.Text = gender?.Name ?? "Не указан";
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки профиля: {ex.Message}");
                return false;
            }
        }
        private void SetupUiForMode()
        {
            if (_isInitialSetup)
            {
                Title = "Первый вход — заполнение профиля";
                WindowTitleText.Text = "Заполните ваш профиль";
                BtnSave.Content = "Сохранить и продолжить";
                BtnCancel.Content = "Отмена";
                TxtLogin.Visibility = Visibility.Collapsed;
                LoginHint.Visibility = Visibility.Collapsed;
                InitialSetupHint.Visibility = Visibility.Visible;
            }
            else
            {
                Title = "Настройки профиля";
                WindowTitleText.Text = "Редактирование профиля администратора";
                BtnSave.Content = "Сохранить изменения";
                BtnCancel.Content = "Отмена";
                TxtLogin.Visibility = Visibility.Visible;
                LoginHint.Visibility = Visibility.Visible;
                InitialSetupHint.Visibility = Visibility.Collapsed;
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ValidateForm();
            if (!ValidateNameField(TxtLastName))
            {
                ShowError("Фамилия обязательна для заполнения.", TxtLastName);
                return;
            }
            if (!ValidateNameField(TxtFirstName))
            {
                ShowError("Имя обязательно для заполнения.", TxtFirstName);
                return;
            }
            if (!ValidateBirthdayField())
            {
                ShowError("Укажите корректную дату рождения.", DpBirthday);
                return;
            }
            if (!ValidatePhoneField())
            {
                ShowError("Телефон должен содержать ровно 10 цифр после +7", TxtPhone);
                return;
            }
            if (!IsPhoneUnique(TxtPhone.Text))
            {
                ShowError("Данный номер телефона уже используется другим пользователем.", TxtPhone);
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
                ShowError("Введите корректный email.", TxtEmail);
                return;
            }
            if (!IsEmailUnique(email))
            {
                ShowError("Данный email уже используется другим пользователем.", TxtEmail);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                if (_isInitialSetup)
                {
                    SaveNewProfile(context, email);
                    MessageBox.Show(
                        "Профиль успешно создан!\n\n Рекомендуется сменить пароль после первого входа.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    UpdateExistingProfile(context, email);
                    MessageBox.Show("Профиль успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveNewProfile(ClinicSoftContext context, string email)
        {
            int? genderId = context.Genders
                .FirstOrDefault(g => g.Name == TxtGender.Text)?.Id;
            var admin = new Models.Admin
            {
                UserId = _userId,
                LastName = TxtLastName.Text.Trim(),
                FirstName = TxtFirstName.Text.Trim(),
                MiddleName = string.IsNullOrWhiteSpace(TxtMiddleName.Text) ? null : TxtMiddleName.Text.Trim(),
                Email = email,
                Phone = FormatPhoneForSave(TxtPhone.Text),
                Birthday = DateOnly.FromDateTime(DpBirthday.SelectedDate.Value.Date),
                GenderCode = genderId ?? 1
            };
            context.Admins.Add(admin);
            context.SaveChanges();
        }
        private void UpdateExistingProfile(ClinicSoftContext context, string email)
        {
            var admin = context.Admins.FirstOrDefault(a => a.UserId == _userId);
            if (admin == null) throw new Exception("Профиль не найден.");
            admin.LastName = TxtLastName.Text.Trim();
            admin.FirstName = TxtFirstName.Text.Trim();
            admin.MiddleName = string.IsNullOrWhiteSpace(TxtMiddleName.Text) ? null : TxtMiddleName.Text.Trim();
            admin.Email = email;
            admin.Phone = FormatPhoneForSave(TxtPhone.Text);
            admin.Birthday = DateOnly.FromDateTime(DpBirthday.SelectedDate.Value.Date);
            context.SaveChanges();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitialSetup)
            {
                var result = MessageBox.Show(
                    "Если вы отмените заполнение профиля, вы не сможете полноценно использовать систему администрирования.\nПродолжить отмену?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    DialogResult = false;
                    Close();
                }
            }
            else
            {
                Close();
            }
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
            if (textBox == null || !_isDataLoaded) return;
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
        private void SetFormattedPhone(string phone)
        {
            TxtPhone.TextChanged -= TxtPhone_TextChanged;
            try
            {
                TxtPhone.Text = FormatPhoneDisplay(phone);
                TxtPhone.SelectionStart = TxtPhone.Text.Length;
                if (_isDataLoaded)
                    ValidatePhoneField();
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
                return _isInitialSetup
                    ? !context.Admins.Any(a => a.Phone == formatted) &&
                      !context.Doctors.Any(d => d.Phone == formatted) &&
                      !context.Patients.Any(p => p.Phone == formatted) &&
                      !context.Registrators.Any(r => r.Phone == formatted)
                    : !context.Admins.Any(a => a.UserId != _userId && a.Phone == formatted) &&
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
            if (_isDataLoaded) ValidateForm();
            return isValid.Value;
        }
        private bool ValidateNameField(TextBox tb)
        {
            bool isValid = !string.IsNullOrWhiteSpace(tb.Text);
            tb.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            tb.BorderThickness = new Thickness(2);
            if (_isDataLoaded) ValidateForm();
            return isValid;
        }
        private void ValidateOptionalField(TextBox tb)
        {
            tb.BorderBrush = _defaultBorderBrush;
            tb.BorderThickness = new Thickness(1);
            if (_isDataLoaded) ValidateForm();
        }
        private bool ValidateEmailField()
        {
            string email = TxtEmail.Text.Trim();
            bool isValid = !string.IsNullOrWhiteSpace(email) && IsValidEmail(email);
            TxtEmail.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            TxtEmail.BorderThickness = new Thickness(2);
            if (_isDataLoaded) ValidateForm();
            return isValid;
        }
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
        private void ValidateForm()
        {
            bool isLastNameValid = !string.IsNullOrWhiteSpace(TxtLastName.Text);
            bool isFirstNameValid = !string.IsNullOrWhiteSpace(TxtFirstName.Text);
            bool isEmailValid = !string.IsNullOrWhiteSpace(TxtEmail.Text) && IsValidEmail(TxtEmail.Text);
            bool isPhoneValid = IsValidPhone(TxtPhone.Text);
            bool isBirthdayValid = DpBirthday.SelectedDate != null && 
                                  DpBirthday.SelectedDate.Value.Date <= DateTime.Today.Date;
            TxtLastName.BorderBrush = isLastNameValid ? _successBorderBrush : _errorBorderBrush;
            TxtLastName.BorderThickness = new Thickness(2); 
            TxtFirstName.BorderBrush = isFirstNameValid ? _successBorderBrush : _errorBorderBrush;
            TxtFirstName.BorderThickness = new Thickness(2);   
            TxtEmail.BorderBrush = isEmailValid ? _successBorderBrush : _errorBorderBrush;
            TxtEmail.BorderThickness = new Thickness(2);   
            TxtPhone.BorderBrush = isPhoneValid ? _successBorderBrush : _errorBorderBrush;
            TxtPhone.BorderThickness = new Thickness(2);  
            DpBirthday.Background = isBirthdayValid ? Brushes.White : new SolidColorBrush(Color.FromRgb(255, 245, 245));
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
                return _isInitialSetup
                    ? !context.Admins.Any(a => a.Email == email) &&
                      !context.Doctors.Any(d => d.Email == email) &&
                      !context.Patients.Any(p => p.Email == email) &&
                      !context.Registrators.Any(r => r.Email == email)
                    : !context.Admins.Any(a => a.UserId != _userId && a.Email == email) &&
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