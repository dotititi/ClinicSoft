using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class CreateDocumentWindow : Window
    {
        private readonly int _doctorId;
        public CreateDocumentWindow(int doctorId)
        {
            InitializeComponent();
            _doctorId = doctorId;
            LoadPatients();
            LoadDocumentTypes();
            CbPatient.SelectionChanged += ValidateForm;
            CbDocumentType.SelectionChanged += ValidateForm;
            TxtContent.TextChanged += ValidateForm;
        }
        private void LoadPatients()
        {
            using var context = new ClinicSoftContext();
            var patients = context.Patients
                .Select(p => new { p.Id, p.LastName, p.FirstName })
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .Select(p => new { p.Id, Display = $"{p.LastName} {p.FirstName}" })
                .ToList();
            CbPatient.ItemsSource = patients;
        }
        private void LoadDocumentTypes()
        {
            using var context = new ClinicSoftContext();
            var templates = context.DocumentTemplates
                .Include(t => t.DocumentType)
                .ToList();

            CbDocumentType.ItemsSource = templates;
        }
        private void CbDocumentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbDocumentType.SelectedItem is DocumentTemplate template)
            {
                TxtContent.Text = !string.IsNullOrWhiteSpace(template.Description)
                    ? template.Description
                    : $"Документ типа: {template.DocumentType?.Name}\n\n[Введите содержимое]";
            }
        }
        private void ValidateForm(object sender, EventArgs e)
        {
            bool isValid = CbPatient.SelectedValue != null &&
                           CbDocumentType.SelectedValue != null &&
                           !string.IsNullOrWhiteSpace(TxtContent.Text);

            BtnCreate.IsEnabled = isValid;
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            string content = TxtContent.Text?.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Содержание документа не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                int patientId = (int)CbPatient.SelectedValue!;
                int templateId = (int)CbDocumentType.SelectedValue!;
                using var context = new ClinicSoftContext();
                var template = (DocumentTemplate)CbDocumentType.SelectedItem!;
                var document = new Document
                {
                    PatientId = patientId,
                    DoctorId = _doctorId,
                    DocumentTemplateId = templateId,
                    Title = $"Документ: {template.DocumentType.Name}",
                    Content = TxtContent.Text.Trim(),
                    CreatedAt = DateTime.Now
                };
                context.Documents.Add(document);
                context.SaveChanges();
                Close();
            }
            catch (Exception ex)
            {
                string message = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при создании документа:\n{message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}