using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSoft.Views.Registrator
{
    public partial class MedicalCardPage : Page
    {
        private readonly int _patientId;
        public MedicalCardPage(int patientId)
        {
            InitializeComponent();
            _patientId = patientId;
            LoadPatientData();
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
        }
        private void LoadPatientData()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var patient = context.Patients
                    .Include(p => p.MedicalCard)
                    .Include(p => p.GenderCodeNavigation)
                    .FirstOrDefault(p => p.Id == _patientId);
                if (patient == null)
                {
                    PatientHeader.Text = "Пациент не найден";
                    return;
                }
                PatientHeader.Text = $"Медицинская карта: {patient.LastName} {patient.FirstName} {patient.MiddleName}".Trim();
                BirthdayText.Text = patient.Birthday.ToString("dd.MM.yyyy");
                GenderText.Text = patient.GenderCodeNavigation?.Name ?? "Не указан";
                PhoneText.Text = patient.Phone ?? "Нет";
                EmailText.Text = patient.Email ?? "Нет";
                InsuranceText.Text = patient.MedicalCard?.InsuranceNumber ?? "Нет";
                AllergiesText.Text = !string.IsNullOrWhiteSpace(patient.MedicalCard?.Allergies)
                    ? patient.MedicalCard.Allergies.Trim()
                    : "Не указано";
                ChronicConditionsText.Text = !string.IsNullOrWhiteSpace(patient.MedicalCard?.ChronicConditions)
                    ? patient.MedicalCard.ChronicConditions.Trim()
                    : "Не указано";
            }
            catch (Exception ex)
            {
                PatientHeader.Text = "Ошибка загрузки данных";
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && NavigationService?.CanGoBack == true)
            {
                GoBack();
                e.Handled = true;
            }
        }
        private void GoBack()
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Нет страниц для возврата.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}