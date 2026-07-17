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

namespace ClinicSoft.Views.PatientViews
{
    public partial class PatientProfileSettingsWindow : Window
    {
        private readonly int _userId;
        private readonly bool _isInitialSetup;
        private bool _isDataLoaded = false;
        private bool _isBirthdayValid = false;
        private int? _existingPatientId;
        private readonly Brush _defaultBorderBrush = new SolidColorBrush(Color.FromRgb(189, 189, 189));
        private readonly Brush _errorBorderBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        private readonly Brush _successBorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));
        public PatientProfileSettingsWindow(int userId, bool isInitialSetup = false)
        {
            InitializeComponent();
            _userId = userId;
            _isInitialSetup = isInitialSetup;
            if (_isInitialSetup)
            {
                HeaderTitle.Text = "Заполните ваш профиль";
                BtnCancel.Content = "Отмена";
                BtnSave.Content = "Сохранить и продолжить";
                Title = "Первый вход — заполнение профиля";
                DpBirthday.SelectedDate = DateTime.Today.AddYears(-30);
                TxtLogin.Visibility = Visibility.Collapsed;
                LoginHint.Visibility = Visibility.Collapsed;
                CbGender.Visibility = Visibility.Visible;
                TxtGender.Visibility = Visibility.Collapsed;
                LoadGenders();
            }
            else
            {
                HeaderTitle.Text = "Редактирование профиля";
                BtnCancel.Content = "Отмена";
                BtnSave.Content = "Сохранить изменения";
                Title = "Настройки профиля";
                TxtLogin.Visibility = Visibility.Visible;
                LoginHint.Visibility = Visibility.Visible;
                CbGender.Visibility = Visibility.Collapsed;
                TxtGender.Visibility = Visibility.Visible;
            }
            TxtEmail.TextChanged += (s, e) => ValidateEmailField();
            TxtLastName.TextChanged += (s, e) => ValidateNameField(TxtLastName);
            TxtFirstName.TextChanged += (s, e) => ValidateNameField(TxtFirstName);
            LoadData();
            _isDataLoaded = true;
        }
        private void LoadData()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var patient = context.Patients
                    .Include(p => p.User)
                    .FirstOrDefault(p => p.UserId == _userId);
                if (patient != null)
                {
                    _existingPatientId = patient.Id;
                    TxtLastName.Text = patient.LastName ?? "";
                    TxtFirstName.Text = patient.FirstName ?? "";
                    TxtMiddleName.Text = patient.MiddleName ?? "";
                    DpBirthday.SelectedDate = patient.Birthday.ToDateTime(TimeOnly.MinValue);
                    TxtEmail.Text = patient.Email ?? "";
                    SetFormattedPhone(patient.Phone ?? "+7 ");
                    TxtLogin.Text = patient.User?.Login ?? "";
                    var gender = context.Genders.FirstOrDefault(g => g.Id == patient.GenderCode);
                    TxtGender.Text = gender?.Name ?? "Не указан";
                }
                else if (!_isInitialSetup)
                {
                    MessageBox.Show(
                        "Ваш профиль не найден. Возможно, требуется настройка администратором.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Close();
                }
                else
                {
                    DpBirthday.SelectedDate = DateTime.Today.AddYears(-30);
                    SetFormattedPhone("+7 ");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                if (!_isInitialSetup) Close();
            }
        }
        private void LoadGenders()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var genders = context.Genders.ToList();
                CbGender.ItemsSource = genders;
                CbGender.DisplayMemberPath = "Name";
                CbGender.SelectedValuePath = "Id";
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
        private void SetFormattedPhone(string phone)
        {
            TxtPhone.TextChanged -= TxtPhone_TextChanged;
            try
            {
                TxtPhone.Text = FormatPhoneDisplay(phone);
                TxtPhone.SelectionStart = TxtPhone.Text.Length;
                ValidatePhoneField();
            }
            finally
            {
                TxtPhone.TextChanged += TxtPhone_TextChanged;
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            bool isLastNameValid = !string.IsNullOrWhiteSpace(TxtLastName.Text);
            ValidateNameField(TxtLastName, isLastNameValid);
            if (!isLastNameValid)
            {
                ShowError("Фамилия обязательна для заполнения.", TxtLastName);
                return;
            }
            bool isFirstNameValid = !string.IsNullOrWhiteSpace(TxtFirstName.Text);
            ValidateNameField(TxtFirstName, isFirstNameValid);
            if (!isFirstNameValid)
            {
                ShowError("Имя обязательно для заполнения.", TxtFirstName);
                return;
            }
            if (!ValidateBirthdayField())
            {
                ShowError("Укажите корректную дату рождения.", DpBirthday);
                return;
            }
            string emailTrimmed = TxtEmail.Text.Trim();
            bool isEmailValid = IsValidEmail(emailTrimmed);
            ValidateEmailField(isEmailValid);
            if (string.IsNullOrWhiteSpace(emailTrimmed))
            {
                ShowError("Email обязателен для заполнения.", TxtEmail);
                return;
            }
            if (!isEmailValid)
            {
                ShowError("Введите корректный email (например: user@example.com).", TxtEmail);
                return;
            }
            if (!IsEmailUnique(emailTrimmed))
            {
                ShowError("Пользователь с таким email уже существует.", TxtEmail);
                return;
            }
            bool isPhoneValid = IsValidPhone(TxtPhone.Text);
            ValidatePhoneField(isPhoneValid);
            if (!isPhoneValid)
            {
                ShowError("Введите корректный телефон (формат: +7 XXX XXX-XX-XX).", TxtPhone);
                return;
            }
            string formattedPhone = FormatPhoneForSave(TxtPhone.Text);
            if (!IsPhoneUnique(formattedPhone))
            {
                ShowError("Пользователь с таким номером телефона уже существует.", TxtPhone);
                return;
            }
            if (_isInitialSetup && CbGender.SelectedValue == null)
            {
                ShowError("Выберите пол.", CbGender);
                return;
            }
            SaveProfile();
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
        private void SaveProfile()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var patient = context.Patients.FirstOrDefault(p => p.UserId == _userId);
                int genderCode = 1; 
                if (_isInitialSetup)
                {
                    if (CbGender.SelectedValue != null)
                    {
                        genderCode = (int)CbGender.SelectedValue;
                    }
                    patient = new Patient
                    {
                        UserId = _userId,
                        LastName = TxtLastName.Text.Trim(),
                        FirstName = TxtFirstName.Text.Trim(),
                        MiddleName = string.IsNullOrWhiteSpace(TxtMiddleName.Text) ? null : TxtMiddleName.Text.Trim(),
                        Birthday = DateOnly.FromDateTime(DpBirthday.SelectedDate.Value.Date),
                        Email = TxtEmail.Text.Trim(),
                        Phone = FormatPhoneForSave(TxtPhone.Text),
                        GenderCode = genderCode 
                    };
                    context.Patients.Add(patient);
                    context.SaveChanges();
                    var medicalCard = new MedicalCard
                    {
                        PatientId = patient.Id,
                        InsuranceNumber = "",
                        Allergies = "Не указано",
                        ChronicConditions = "Не указано"
                    };
                    context.MedicalCards.Add(medicalCard);
                }
                else if (patient != null)
                {
                    patient.LastName = TxtLastName.Text.Trim();
                    patient.FirstName = TxtFirstName.Text.Trim();
                    patient.MiddleName = string.IsNullOrWhiteSpace(TxtMiddleName.Text) ? null : TxtMiddleName.Text.Trim();
                    patient.Birthday = DateOnly.FromDateTime(DpBirthday.SelectedDate.Value.Date);
                    patient.Email = TxtEmail.Text.Trim();
                    patient.Phone = FormatPhoneForSave(TxtPhone.Text);
                }
                else
                {
                    MessageBox.Show("Ошибка: профиль не может быть создан.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                context.SaveChanges();
                string message = _isInitialSetup
                    ? "Профиль успешно создан! Теперь вы можете пользоваться системой."
                    : "Профиль успешно обновлён!";
                MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitialSetup)
            {
                var result = MessageBox.Show(
                    "Если вы отмените заполнение профиля, вы не сможете войти в систему.\nПродолжить отмену?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
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
        private bool IsValidPhone(string phone)
        {
            var digitsOnly = Regex.Replace(phone, @"[^0-9]", "");
            return digitsOnly.Length == 11 && digitsOnly.StartsWith("7");
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
        private bool IsEmailUnique(string email)
        {
            try
            {
                using var context = new ClinicSoftContext();
                return !context.Patients
                    .Any(p => p.Email == email &&
                             (_existingPatientId == null || p.Id != _existingPatientId));
            }
            catch
            {
                return false;
            }
        }
        private bool IsPhoneUnique(string phone)
        {
            try
            {
                using var context = new ClinicSoftContext();
                return !context.Patients
                    .Any(p => p.Phone == phone &&
                             (_existingPatientId == null || p.Id != _existingPatientId));
            }
            catch
            {
                return false;
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
            if (!_isDataLoaded) return;

            var textBox = sender as TextBox;
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
                if (digitsOnly.Length > 0 && digitsOnly[0] == '8') digitsOnly = "7" + digitsOnly.Substring(1);
                if (digitsOnly.Length > 11) digitsOnly = digitsOnly.Substring(0, 11);
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
        private void ValidatePhoneField(bool? isValid = null)
        {
            if (isValid == null)
                isValid = IsValidPhone(TxtPhone.Text);
            TxtPhone.BorderBrush = isValid.Value ? _successBorderBrush : _errorBorderBrush;
            TxtPhone.BorderThickness = new Thickness(2);
        }
        private void ValidateEmailField(bool? isValid = null)
        {
            if (isValid == null)
                isValid = IsValidEmail(TxtEmail.Text);
            TxtEmail.BorderBrush = isValid.Value ? _successBorderBrush : _errorBorderBrush;
            TxtEmail.BorderThickness = new Thickness(2);
        }
        private void ValidateNameField(TextBox tb, bool? isValid = null)
        {
            if (isValid == null)
                isValid = !string.IsNullOrWhiteSpace(tb.Text);
            tb.BorderBrush = isValid.Value ? _successBorderBrush : _errorBorderBrush;
            tb.BorderThickness = new Thickness(2);
        }
        private void ShowError(string message, Control control)
        {
            MessageBox.Show(message, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            control.Focus();
        }
    }
}