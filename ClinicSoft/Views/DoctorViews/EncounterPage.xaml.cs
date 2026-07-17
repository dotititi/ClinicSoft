using ClinicSoft.Data;
using ClinicSoft.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSoft.Views.DoctorViews
{
    public partial class EncounterPage : Page
    {
        private readonly int _appointmentId;
        private readonly bool _isDoctor;
        private Visit? _existingVisit;
        private Appointment? _appointment;
        private readonly Dictionary<int, bool> _labTestHasResults = new();
        private bool _isCompletedMode = false;
        private List<Diagnosis> _allDiagnoses = new();
        private List<LabTestType> _allLabTests = new();
        private class MedicationItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string? FixedDosage { get; set; }
        }
        private List<MedicationItem> _allMedications = new();
        private Diagnosis? _selectedDiagnosis;
        private readonly List<(int MedicationId, string Name, string Dosage, int DurationDays, string Instructions)> _prescribedMedications = new();
        private readonly ObservableCollection<dynamic> _selectedLabTests = new(); 
        public EncounterPage(int appointmentId, bool isDoctor = true)
        {
            InitializeComponent();
            SelectedLabTestsList.ItemsSource = _selectedLabTests;
            _appointmentId = appointmentId;
            _isDoctor = isDoctor;
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                using var context = new ClinicSoftContext();
                _appointment = context.Appointments
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.MedicalCard)
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.GenderCodeNavigation)
                    .First(a => a.Id == _appointmentId);

                _isCompletedMode = _appointment.Status == "completed";
                Title = _isCompletedMode ? "Просмотр приёма" : "Приём пациента";
                var patient = _appointment.Patient;
                PatientHeader.Text = $"{patient.LastName} {patient.FirstName} {patient.MiddleName}".Trim();
                AppointmentTimeText.Text = _appointment.ScheduledTime.ToString("dd.MM.yyyy HH:mm");
                ReasonText.Text = _appointment.Reason ?? "Не указана";
                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - patient.Birthday.Year;
                if (patient.Birthday > today.AddYears(-age))
                    age--;
                AgeText.Text = $"{age} лет";
                GenderText.Text = patient.GenderCodeNavigation?.Name ?? "Не указан";
                InsuranceText.Text = patient.MedicalCard?.InsuranceNumber ?? "Нет";
                PhoneText.Text = patient.Phone ?? "Нет";
                var allergies = patient.MedicalCard?.Allergies?.Trim() ?? "Нет";
                var chronicConditions = patient.MedicalCard?.ChronicConditions?.Trim() ?? "Нет";
                MedicalHistoryText.Text = $"Аллергии: {allergies}\nХронические болезни: {chronicConditions}";
                _allDiagnoses = context.Diagnoses.ToList();
                CbDiagnosis.ItemsSource = _allDiagnoses;
                CbDiagnosis.DisplayMemberPath = "Name";
                CbDiagnosis.SelectedValuePath = "Id";
                _allMedications = context.Medications
                    .Include(m => m.DosageForm)
                    .ToList()
                    .Select(m => new MedicationItem
                    {
                        Id = m.Id,
                        Name = m.Name,
                        FixedDosage = m.DosageForm?.Name
                    })
                    .ToList();
                CbMedication.ItemsSource = _allMedications;
                CbMedication.DisplayMemberPath = "Name";
                CbMedication.SelectedValuePath = "Id";
                _allLabTests = context.LabTestTypes.ToList();
                CbLabTest.ItemsSource = _allLabTests;
                CbLabTest.DisplayMemberPath = "Name";
                CbLabTest.SelectedValuePath = "Id";
                _existingVisit = context.Visits
                    .Include(v => v.Diagnosis)
                    .FirstOrDefault(v => v.AppointmentId == _appointmentId);
                if (_existingVisit != null)
                {
                    ComplaintsBox.Text = _existingVisit.ChiefComplaint ?? "";
                    ComplaintsBox.IsReadOnly = !_isDoctor || !_isCompletedMode;
                    if (_existingVisit.Diagnosis != null)
                    {
                        _selectedDiagnosis = _existingVisit.Diagnosis;
                        DiagnosisDisplayText.Text = _selectedDiagnosis.Name;
                        SelectedDiagnosisPanel.Visibility = System.Windows.Visibility.Visible;
                    }
                    var prescription = context.TreatmentPlans
                        .Include(p => p.PrescribedMedications)
                            .ThenInclude(pm => pm.Medication)
                                .ThenInclude(m => m.DosageForm)
                        .FirstOrDefault(p => p.VisitId == _existingVisit.Id);
                    if (prescription != null)
                    {
                        TreatmentPlanBox.Text = prescription.Notes ?? "";
                        TreatmentPlanBox.IsReadOnly = !_isDoctor || !_isCompletedMode;
                        _prescribedMedications.Clear();
                        foreach (var pm in prescription.PrescribedMedications)
                        {
                            string medName = pm.Medication?.DosageForm != null
                                ? $"{pm.Medication.Name} ({pm.Medication.DosageForm.Name})"
                                : pm.Medication?.Name ?? "Неизвестно";
                            _prescribedMedications.Add((
                                MedicationId: pm.MedicationId,
                                Name: medName,
                                Dosage: pm.Dosage ?? "",
                                DurationDays: pm.DurationDays,
                                Instructions: pm.Instructions ?? "Инструкция не указана"
                            ));
                        }
                        UpdateMedicationsList();
                    }
                    var labOrder = context.LabOrders
                        .Include(lo => lo.LabOrderItems)
                            .ThenInclude(loi => loi.TestType)
                        .FirstOrDefault(lo => lo.VisitId == _existingVisit.Id);
                    if (labOrder != null)
                    {
                        _selectedLabTests.Clear();
                        _labTestHasResults.Clear();
                        foreach (var item in labOrder.LabOrderItems)
                        {
                            if (item.TestType != null)
                            {
                                bool hasResults = context.LabResultItems
                                    .Any(r => r.LabResult.LabOrderId == labOrder.Id && r.TestTypeId == item.TestTypeId);
                                _labTestHasResults[item.TestTypeId] = hasResults;
                                bool canEdit = _isDoctor && !hasResults;
                                _selectedLabTests.Add(new
                                {
                                    Id = item.TestTypeId,
                                    Name = item.TestType.Name,
                                    CanEdit = canEdit
                                });
                            }
                        }
                    }
                }
                else
                {
                    _prescribedMedications.Clear();
                    _selectedLabTests.Clear();
                    _selectedDiagnosis = null;
                    SelectedDiagnosisPanel.Visibility = System.Windows.Visibility.Collapsed;
                    ComplaintsBox.Text = "";
                    TreatmentPlanBox.Text = "";
                    UpdateMedicationsList();
                }
                ConfigureUIForMode();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ConfigureUIForMode()
        {
            bool canEdit = _isDoctor;
            if (_isCompletedMode)
            {
                BtnSave.Content = "Сохранить изменения";
                BtnSave.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                BtnSave.Content = "Сохранить и завершить";
                BtnSave.Visibility = _isDoctor ? Visibility.Visible : Visibility.Collapsed;
            }
            CbDiagnosis.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            CbMedication.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            CbLabTest.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            BtnClearDiagnosis.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (!_isDoctor)
            {
                if (BtnCancel != null)
                {
                    BtnCancel.Content = "Назад";
                }
            }
        }
        private void UpdateMedicationsList()
        {
            bool canEdit = _isDoctor;
            MedicationsListView.ItemsSource = _prescribedMedications.Select(m => new
            {
                MedicationId = m.MedicationId,
                Name = m.Name,
                Dosage = m.Dosage,
                Duration = $"{m.DurationDays} дн.",
                Instructions = m.Instructions.Length > 30
                    ? m.Instructions.Substring(0, 27) + "..."
                    : m.Instructions,
                FullInstructions = m.Instructions,
                CanEdit = canEdit
            }).ToList();
        }
        private void CbDiagnosis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Diagnosis diagnosis)
            {
                SetDiagnosis(diagnosis);
                CbDiagnosis.SelectedIndex = -1;
                CbDiagnosis.Text = "";
            }
        }
        private void CbDiagnosis_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox cb && !string.IsNullOrEmpty(cb.Text) && cb.SelectedItem == null)
            {
                var filtered = _allDiagnoses
                    .Where(d => d.Name.Contains(cb.Text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (filtered.Count == 1)
                {
                    SetDiagnosis(filtered[0]);
                    cb.Text = "";
                }
                else if (filtered.Count > 1)
                {
                    cb.ItemsSource = filtered;
                    cb.IsDropDownOpen = true;
                }
            }
        }
        private void SetDiagnosis(Diagnosis diagnosis)
        {
            _selectedDiagnosis = diagnosis;
            DiagnosisDisplayText.Text = diagnosis.Name;
            SelectedDiagnosisPanel.Visibility = System.Windows.Visibility.Visible;
        }
        private void BtnClearDiagnosis_Click(object sender, RoutedEventArgs e)
        {
            _selectedDiagnosis = null;
            SelectedDiagnosisPanel.Visibility = System.Windows.Visibility.Collapsed;
        }
        private void CbMedication_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbMedication.SelectedItem is MedicationItem selectedMed)
            {
                ShowMedicationDialog(selectedMed);
                CbMedication.SelectedIndex = -1;
                CbMedication.Text = "";
            }
        }
        private void CbMedication_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox cb && !string.IsNullOrEmpty(cb.Text) && cb.SelectedItem == null)
            {
                var filtered = _allMedications
                    .Where(m => m.Name.Contains(cb.Text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (filtered.Count == 1)
                {
                    cb.SelectedItem = filtered[0];
                    ShowMedicationDialog(filtered[0]);
                    cb.Text = "";
                }
                else if (filtered.Count > 1)
                {
                    cb.ItemsSource = filtered;
                    cb.IsDropDownOpen = true;
                }
            }
        }
        private void ShowMedicationDialog(MedicationItem medication)
        {
            ShowMedicationDialog(medication, null, 7, "");
        }
        private void ShowMedicationDialog(MedicationItem medication, int? medicationId = null, int durationDays = 7, string instructions = "")
        {
            var dialog = new Window
            {
                Title = medicationId.HasValue
                    ? $"Редактировать: {medication.Name}"
                    : $"Назначить: {medication.Name}",
                Width = 500,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                Background = System.Windows.Media.Brushes.White,
                ResizeMode = ResizeMode.NoResize
            };
            var stack = new StackPanel { Margin = new Thickness(20) };
            if (!string.IsNullOrEmpty(medication.FixedDosage))
            {
                stack.Children.Add(new TextBlock { Text = "Форма препарата", Margin = new Thickness(0, 0, 0, 4) });
                var dosageDisplay = new TextBox
                {
                    Text = medication.FixedDosage,
                    IsReadOnly = true,
                    Background = System.Windows.Media.Brushes.LightGray,
                    Margin = new Thickness(0, 0, 0, 16)
                };
                stack.Children.Add(dosageDisplay);
            }
            stack.Children.Add(new TextBlock { Text = "Длительность (дней) *", Margin = new Thickness(0, 0, 0, 4) });
            var durationBox = new TextBox { Text = durationDays.ToString(), Margin = new Thickness(0, 0, 0, 16) };
            stack.Children.Add(durationBox);
            stack.Children.Add(new TextBlock
            {
                Text = "Инструкция по применению *",
                FontWeight = System.Windows.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            var instructionsBox = new TextBox
            {
                Text = instructions,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 100,
                Margin = new Thickness(0, 0, 0, 24),
                FontSize = 12,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            stack.Children.Add(instructionsBox);
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnCancel = new Button { Content = "Отмена", Width = 80 };
            var btnAction = new Button
            {
                Content = medicationId.HasValue ? "Сохранить изменения" : "Добавить в назначение",
                Width = 180,
                Margin = new Thickness(12, 0, 0, 0),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(25, 118, 210)),
                Foreground = System.Windows.Media.Brushes.White
            };
            btnAction.Click += (s, ev) =>
            {
                try {
                if (!int.TryParse(durationBox.Text, out int days) || days <= 0)
                {
                    MessageBox.Show("Укажите корректную длительность.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(instructionsBox.Text))
                {
                    MessageBox.Show("Укажите инструкцию по применению.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool wasAdded = false;

                if (medicationId.HasValue)
                {
                    var index = _prescribedMedications.FindIndex(m => m.MedicationId == medicationId.Value);
                    if (index >= 0)
                    {
                        _prescribedMedications[index] = (
                            MedicationId: medicationId.Value,
                            Name: medication.Name,
                            Dosage: medication.FixedDosage ?? "",
                            DurationDays: days,
                            Instructions: instructionsBox.Text.Trim()
                        );
                        wasAdded = true;
                    }
                }
                else
                {
                    if (!_prescribedMedications.Any(m => m.MedicationId == medication.Id))
                    {
                        _prescribedMedications.Add((
                            MedicationId: medication.Id,
                            Name: medication.Name,
                            Dosage: medication.FixedDosage ?? "",
                            DurationDays: days,
                            Instructions: instructionsBox.Text.Trim()
                        ));
                        wasAdded = true;
                    }
                    else
                    {
                        MessageBox.Show($"Препарат \"{medication.Name}\" уже добавлен в назначения.",
                                        "Предупреждение",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                        return;
                    }
                }

                if (wasAdded)
                {
                    UpdateMedicationsList();
                    dialog.DialogResult = true;
                }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Внутренняя ошибка: {ex.Message}", "Ошибка");
                    dialog.Close();
                }
            };
            btnCancel.Click += (s, ev) => dialog.Close();
            btnPanel.Children.Add(btnCancel);
            btnPanel.Children.Add(btnAction);
            stack.Children.Add(btnPanel);
            dialog.Content = stack;
            dialog.ShowDialog();
        }
        private void EditMedication_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int medicationId)
            {
                var index = _prescribedMedications.FindIndex(m => m.MedicationId == medicationId);
                if (index >= 0)
                {
                    var existing = _prescribedMedications[index];
                    var medicationItem = _allMedications.FirstOrDefault(m => m.Id == medicationId);
                    if (medicationItem != null)
                    {
                        ShowMedicationDialog(
                            medicationItem,
                            medicationId,
                            existing.DurationDays,
                            existing.Instructions
                        );
                    }
                }
            }
        }
        private void RemoveMedication_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int medicationId)
            {
                _prescribedMedications.RemoveAll(m => m.MedicationId == medicationId);
                UpdateMedicationsList();
            }
        }
        private void CbLabTest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbLabTest.SelectedItem is LabTestType test)
            {
                AddLabTest(test);
                CbLabTest.Text = "";
                CbLabTest.SelectedIndex = -1;
            }
        }
        private void CbLabTest_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox cb && !string.IsNullOrEmpty(cb.Text) && cb.SelectedItem == null)
            {
                var filtered = _allLabTests
                    .Where(t => t.Name.Contains(cb.Text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (filtered.Count == 1)
                {
                    AddLabTest(filtered[0]);
                    cb.Text = "";
                }
                else if (filtered.Count > 1)
                {
                    cb.ItemsSource = filtered;
                    cb.IsDropDownOpen = true;
                }
            }
        }
        private void AddLabTest(LabTestType test)
        {
            if (!_selectedLabTests.Cast<dynamic>().Any(t => (int)t.Id == test.Id))
            {
                bool canEdit = _isDoctor;
                _selectedLabTests.Add(new
                {
                    Id = test.Id,
                    Name = test.Name,
                    CanEdit = canEdit
                });
            }
            else
            {
                MessageBox.Show($"Анализ \"{test.Name}\" уже добавлен.",
                                "Предупреждение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }
        private void RemoveLabTest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int testId)
            {
                if (_labTestHasResults.TryGetValue(testId, out bool hasResults) && hasResults)
                {
                    MessageBox.Show(
                        "Нельзя удалить анализ, так как по нему уже есть результаты.",
                        "Ограничение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                var toRemove = _selectedLabTests.Cast<dynamic>().FirstOrDefault(t => (int)t.Id == testId);
                if (toRemove != null)
                {
                    _selectedLabTests.Remove(toRemove);
                    _labTestHasResults.Remove(testId);
                }
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDiagnosis == null)
            {
                MessageBox.Show("Укажите диагноз перед сохранением приёма.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string complaints = ComplaintsBox.Text?.Trim();
            if (string.IsNullOrEmpty(complaints))
            {
                MessageBox.Show("Укажите жалобы пациента перед сохранением приёма.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                ComplaintsBox.Focus();
                return;
            }
            try
            {
                using var context = new ClinicSoftContext();
                var appointment = context.Appointments.Find(_appointmentId);
                if (appointment == null) return;
                appointment.Status = "completed";
                var visit = context.Visits
                    .FirstOrDefault(v => v.AppointmentId == _appointmentId);
                if (visit == null)
                {
                    visit = new Visit
                    {
                        PatientId = appointment.PatientId,
                        DoctorId = appointment.DoctorId,
                        AppointmentId = _appointmentId,
                        VisitTime = DateTime.Now,
                        ChiefComplaint = complaints,
                        DiagnosisId = _selectedDiagnosis.Id
                    };
                    context.Visits.Add(visit);
                }
                else
                {
                    visit.ChiefComplaint = complaints;
                    visit.DiagnosisId = _selectedDiagnosis.Id;
                }
                var oldLabOrder = context.LabOrders.FirstOrDefault(lo => lo.VisitId == visit.Id);
                if (oldLabOrder != null)
                {
                    context.LabOrderItems.RemoveRange(context.LabOrderItems.Where(loi => loi.LabOrderId == oldLabOrder.Id));
                    context.LabOrders.Remove(oldLabOrder);
                }
                var oldPrescription = context.TreatmentPlans.FirstOrDefault(p => p.VisitId == visit.Id);
                if (oldPrescription != null)
                {
                    context.PrescribedMedications.RemoveRange(
                        context.PrescribedMedications.Where(pm => pm.TreatmentPlanId == oldPrescription.Id));
                    context.TreatmentPlans.Remove(oldPrescription);
                }
                context.SaveChanges();
                if (_selectedLabTests.Cast<dynamic>().Any())
                {
                    var labOrder = new LabOrder
                    {
                        PatientId = appointment.PatientId,
                        DoctorId = appointment.DoctorId,
                        VisitId = visit.Id,
                        OrderedAt = DateTime.Now,
                        Status = "in_progress"
                    };
                    context.LabOrders.Add(labOrder);
                    context.SaveChanges();
                    foreach (dynamic test in _selectedLabTests)
                    {
                        context.LabOrderItems.Add(new LabOrderItem
                        {
                            LabOrderId = labOrder.Id,
                            TestTypeId = test.Id
                        });
                    }
                }
                if (_prescribedMedications.Any())
                {
                    var treatmentPlan = new TreatmentPlan
                    {
                        VisitId = visit.Id,
                        DoctorId = appointment.DoctorId,
                        IssuedAt = DateTime.Now,
                        Status = "active",
                        Notes = TreatmentPlanBox.Text?.Trim()
                    };
                    context.TreatmentPlans.Add(treatmentPlan);
                    context.SaveChanges();
                    foreach (var item in _prescribedMedications)
                    {
                        context.PrescribedMedications.Add(new PrescribedMedication
                        {
                            TreatmentPlanId = treatmentPlan.Id,
                            MedicationId = item.MedicationId,
                            Dosage = item.Dosage,
                            DurationDays = item.DurationDays,
                            Instructions = item.Instructions
                        });
                    }
                }
                context.SaveChanges();
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.InnerException?.Message ?? ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}