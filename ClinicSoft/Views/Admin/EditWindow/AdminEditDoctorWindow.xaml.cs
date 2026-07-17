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

namespace ClinicSoft.Views.Admin.EditWindow
{
    public partial class AdminEditDoctorWindow : Window
    {
        private readonly int _doctorId;
        private readonly Brush _defaultBorderBrush = new SolidColorBrush(Color.FromRgb(189, 189, 189));
        private readonly Brush _errorBorderBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        private readonly Brush _successBorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));
        private bool _isBirthdayValid = false;
        private bool _isInitialLoad = true;
        public AdminEditDoctorWindow(int doctorId)
        {
            InitializeComponent();
            _doctorId = doctorId;
            LoadLookupData();
            LoadDoctorData();
            TxtLastName.TextChanged += (s, e) => ValidateNameField(TxtLastName);
            TxtFirstName.TextChanged += (s, e) => ValidateNameField(TxtFirstName);
            TxtMiddleName.TextChanged += (s, e) => ValidateNameField(TxtMiddleName);
            TxtEmail.TextChanged += (s, e) => ValidateEmailField();
            TxtMiddleName.TextChanged += (s, e) => ValidateOptionalField(TxtMiddleName);
            DpBirthday.SelectedDateChanged += (s, e) => ValidateBirthdayField();
            TxtPhone.TextChanged += (s, e) => ValidatePhoneField();
            CbGender.SelectionChanged += (s, e) => ValidateGenderField();
            _isInitialLoad = false;
        }
        private void LoadLookupData()
        {
            using var context = new ClinicSoftContext();
            CbSpeciality.ItemsSource = context.MedicalSpecialities.ToList();
            CbSpeciality.DisplayMemberPath = "Name";
            CbSpeciality.SelectedValuePath = "Id";
            CbDepartment.ItemsSource = context.Departments.ToList();
            CbDepartment.DisplayMemberPath = "Name";
            CbDepartment.SelectedValuePath = "Id";
            CbOffice.ItemsSource = context.Offices.ToList();
            CbOffice.DisplayMemberPath = "Number";
            CbOffice.SelectedValuePath = "Id";
            CbStatus.ItemsSource = context.DoctorStatuses.ToList();
            CbStatus.DisplayMemberPath = "Name";
            CbStatus.SelectedValuePath = "Id";
            CbGender.ItemsSource = context.Genders.ToList();
            CbGender.DisplayMemberPath = "Name";
            CbGender.SelectedValuePath = "Id";
        }
        private void LoadDoctorData()
        {
            using var context = new ClinicSoftContext();
            var doctor = context.Doctors
                .Include(d => d.Speciality)
                .Include(d => d.Department)
                .Include(d => d.Office)
                .Include(d => d.Status)
                .Include(d => d.User)
                .FirstOrDefault(d => d.Id == _doctorId);
            if (doctor == null)
            {
                MessageBox.Show("Врач не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            TxtLastName.Text = doctor.LastName ?? "";
            TxtFirstName.Text = doctor.FirstName ?? "";
            TxtMiddleName.Text = doctor.MiddleName ?? "";
            DpBirthday.SelectedDate = doctor.Birthday.ToDateTime(TimeOnly.MinValue);
            TxtEmail.Text = doctor.Email ?? "";
            SetFormattedPhone(doctor.Phone ?? "+7 ");
            TxtLogin.Text = doctor.User?.Login ?? "";
            CbSpeciality.SelectedValue = doctor.SpecialityId;
            CbDepartment.SelectedValue = doctor.DepartmentId;
            CbOffice.SelectedValue = doctor.OfficeId;
            CbStatus.SelectedValue = doctor.StatusId;
            CbGender.SelectedValue = doctor.GenderCode;
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
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
            if (CbOffice.SelectedValue == null)
            {
                MessageBox.Show("Выберите кабинет.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CbStatus.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                var doctor = context.Doctors.FirstOrDefault(d => d.Id == _doctorId);
                if (doctor == null)
                {
                    MessageBox.Show("Врач не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                doctor.LastName = TxtLastName.Text.Trim();
                doctor.FirstName = TxtFirstName.Text.Trim();
                doctor.MiddleName = TxtMiddleName.Text.Trim();
                doctor.Birthday = DateOnly.FromDateTime(DpBirthday.SelectedDate.Value.Date);
                doctor.Email = email;
                doctor.Phone = FormatPhoneForSave(TxtPhone.Text);
                doctor.GenderCode = (int)CbGender.SelectedValue;
                doctor.SpecialityId = (int)CbSpeciality.SelectedValue;
                doctor.DepartmentId = (int)CbDepartment.SelectedValue;
                doctor.OfficeId = (int)CbOffice.SelectedValue;
                doctor.StatusId = (int)CbStatus.SelectedValue;
                context.SaveChanges();
                MessageBox.Show("Данные врача успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool ValidateGenderField()
        {
            bool isValid = CbGender.SelectedValue != null;
            CbGender.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            CbGender.BorderThickness = new Thickness(2);
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
                if (!_isInitialLoad)
                    ValidatePhoneField();
            }
            finally
            {
                TxtPhone.TextChanged += TxtPhone_TextChanged;
            }
        }
        private void ValidateOptionalField(TextBox tb)
        {
            tb.BorderBrush = _defaultBorderBrush;
            tb.BorderThickness = new Thickness(1);
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
                return !context.Doctors.Any(d => d.Id != _doctorId && d.Phone == formatted) &&
                       !context.Patients.Any(p => p.Phone == formatted) &&
                       !context.Admins.Any(a => a.Phone == formatted) &&
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
            return isValid.Value;
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
                return !context.Doctors.Any(d => d.Id != _doctorId && d.Email == email) &&
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
        private bool ValidateNameField(TextBox tb)
        {
            bool isValid = !string.IsNullOrWhiteSpace(tb.Text);
            tb.BorderBrush = isValid ? _successBorderBrush : _errorBorderBrush;
            tb.BorderThickness = new Thickness(2);
            return isValid;
        }
        private void ShowError(string message, Control control)
        {
            MessageBox.Show(message, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            control.Focus();
        }
    }
}