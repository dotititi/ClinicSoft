using ClinicSoft.Data;
using ClinicSoft.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;

namespace ClinicSoft.Views.Admin
{
    /// <summary>
    /// Логика взаимодействия для AppointmentPage.xaml
    /// </summary>
    public partial class AppointmentPage : Page
    {
        private ClinicSoftContext _context = new();

        public AppointmentPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            // Пациенты
            var patients = _context.Patients
                .Select(p => new
                {
                    Id = p.Id,
                    Display = $"{p.LastName} {p.FirstName} {p.MiddleName}".Trim()
                })
                .ToList();
            CbPatient.ItemsSource = patients;
            CbPatient.DisplayMemberPath = "Display";
            CbPatient.SelectedValuePath = "Id";

            // Врачи
            var doctors = _context.Doctors
                .Select(d => new
                {
                    Id = d.Id,
                    Display = $"{d.LastName} {d.FirstName} {d.MiddleName}".Trim()
                })
                .ToList();
            CbDoctor.ItemsSource = doctors;
            CbDoctor.DisplayMemberPath = "Display";
            CbDoctor.SelectedValuePath = "Id";

            // Записи
            var appointments = _context.Appointments
                .Where(a => a.ScheduledTime >= DateTime.Today)
                .OrderBy(a => a.ScheduledTime)
                .Select(a => new
                {
                    a.ScheduledTime,
                    PatientName = $"{a.Patient.LastName} {a.Patient.FirstName} {a.Patient.MiddleName}".Trim(),
                    DoctorName = $"{a.Doctor.LastName} {a.Doctor.FirstName} {a.Doctor.MiddleName}".Trim(),
                    a.Status
                })
                .ToList();

            AppointmentDataGrid.ItemsSource = appointments;
        }

        private void DpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DpDate.SelectedDate.HasValue)
            {
                // Генерируем слоты: 9:00, 9:30, ..., 18:00
                var times = new string[]
                {
                    "09:00", "09:30", "10:00", "10:30", "11:00", "11:30",
                    "12:00", "12:30", "13:00", "13:30", "14:00", "14:30",
                    "15:00", "15:30", "16:00", "16:30", "17:00", "17:30"
                };
                CbTime.ItemsSource = times;
                CbTime.SelectedIndex = 0;
            }
        }

        private void BtnCreateAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (CbPatient.SelectedItem == null || CbDoctor.SelectedItem == null ||
                !DpDate.SelectedDate.HasValue || CbTime.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            var patient = (global::ClinicSoft.Models.Patient)CbPatient.SelectedItem;
            var doctor = (global::ClinicSoft.Models.Doctor)CbDoctor.SelectedItem;
            var timeStr = (string)CbTime.SelectedItem;
            var dateTime = DpDate.SelectedDate.Value.Date + TimeSpan.Parse(timeStr);

            // Проверка: нет ли уже записи у этого врача в это время
            var conflict = _context.Appointments
                .Any(a => a.DoctorId == doctor.Id &&
                          a.ScheduledTime.Date == dateTime.Date &&
                          a.ScheduledTime.Hour == dateTime.Hour &&
                          a.ScheduledTime.Minute == dateTime.Minute);

            if (conflict)
            {
                MessageBox.Show("Врач уже занят в это время.");
                return;
            }

            var appointment = new Appointment
            {
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                ScheduledTime = dateTime,
                Reason = TxtReason.Text,
                Status = "scheduled"
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            MessageBox.Show("Пациент успешно записан!");
            LoadData(); // Обновить список
        }
    }
}
