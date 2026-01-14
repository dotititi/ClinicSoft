using ClinicSoft.Data;
using ClinicSoft.Models;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClinicSoft.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для AddPatientWindow.xaml
    /// </summary>
    public partial class AddPatientWindow : Window
    {
        public AddPatientWindow()
        {
            InitializeComponent();
            CbGender.SelectedIndex = 0;
            DpBirthday.SelectedDate = DateTime.Today.AddYears(-30);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtLastName.Text) ||
                string.IsNullOrWhiteSpace(TxtFirstName.Text))
            {
                MessageBox.Show("Заполните ФИО.");
                return;
            }

            using var context = new ClinicSoftContext();

            // 1. Создаём пользователя
            var user = new User
            {
                Login = $"{TxtLastName.Text}_{Guid.NewGuid():N}".Substring(0, 50),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("default123"), // временный пароль
                Role = "patient"
            };
            context.Users.Add(user);
            context.SaveChanges();

            // 2. Создаём пациента
            var patient = new Patient
            {
                UserId = user.Id,
                LastName = TxtLastName.Text,
                FirstName = TxtFirstName.Text,
                MiddleName = TxtMiddleName.Text,
                Birthday = DateOnly.FromDateTime(DpBirthday.SelectedDate.Value),
                GenderCode = (CbGender.SelectedItem as ComboBoxItem).Tag.ToString()[0],
                Phone = TxtPhone.Text,
                Email = TxtEmail.Text
            };
            context.Patients.Add(patient);
            context.SaveChanges();

            // 3. Создаём медкарту
            if (!string.IsNullOrWhiteSpace(TxtInsurance.Text) ||
                !string.IsNullOrWhiteSpace(TxtAllergies.Text) ||
                !string.IsNullOrWhiteSpace(TxtChronic.Text))
            {
                var card = new MedicalCard
                {
                    PatientId = patient.Id,
                    InsuranceNumber = TxtInsurance.Text,
                    Allergies = TxtAllergies.Text,
                    ChronicConditions = TxtChronic.Text
                };
                context.MedicalCards.Add(card);
                context.SaveChanges();
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        // Удаляем подсказки при вводе (опционально)
        private void DpBirthday_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Можно убрать placeholder-эффект, если реализован
        }
    }
}
