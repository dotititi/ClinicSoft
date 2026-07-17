using ClinicSoft.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class DocumentViewWindow : Window
    {
        private readonly int _documentId;
        private bool _isEditable;
        public DocumentViewWindow(int documentId, bool isEditable = false)
        {
            InitializeComponent();
            _documentId = documentId;
            _isEditable = isEditable;
            LoadDocument();
            ContentText.IsReadOnly = !_isEditable;
            BtnSave.Visibility = _isEditable ? Visibility.Visible : Visibility.Collapsed;
            Title = _isEditable ? "Редактирование документа" : "Просмотр документа";
        }
        private void LoadDocument()
        {
            try
            {
                using var context = new ClinicSoftContext();
                var doc = context.Documents
                    .Include(d => d.Patient)
                    .Include(d => d.DocumentTemplate)
                        .ThenInclude(t => t.DocumentType)
                    .FirstOrDefault(d => d.Id == _documentId);
                if (doc == null)
                {
                    MessageBox.Show("Документ не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Close();
                    return;
                }
                TitleText.Text = doc.Title;
                PatientText.Text = $"Пациент: {doc.Patient.LastName} {doc.Patient.FirstName}";
                TypeText.Text = $"Тип: {doc.DocumentTemplate?.DocumentType?.Name ?? "Не указан"}";
                DateText.Text = $"Дата создания: {doc.CreatedAt:dd.MM.yyyy HH:mm}";
                ContentText.Text = doc.Content;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки документа:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var context = new ClinicSoftContext();
                var doc = context.Documents.Find(_documentId);
                if (doc == null) return;
                doc.Content = ContentText.Text.Trim();
                context.SaveChanges();
                MessageBox.Show("Документ успешно сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}