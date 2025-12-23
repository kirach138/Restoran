using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class StatusViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Status _selectedStatus;

        public ObservableCollection<Status> Statuses { get; set; }
        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }

        public Status SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
            }
        }

        public StatusViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            Statuses = new ObservableCollection<Status>();
            LoadStatuses();

            AddCommand = new RelayCommand(AddStatus);
            DeleteCommand = new RelayCommand(DeleteStatus, CanDeleteStatus);
            SaveCommand = new RelayCommand(SaveStatuses);
        }

        private void LoadStatuses()
        {
            Statuses.Clear();
            foreach (var status in _context.Status.ToList())
            {
                Statuses.Add(status);
            }
        }

        private void AddStatus(object parameter)
        {
            var newStatus = new Status
            {
                ID = GetNextId(),
                Name = "Новый статус"
            };

            Statuses.Add(newStatus);
            _context.Status.Add(newStatus);
            SelectedStatus = newStatus;
        }

        private int GetNextId()
        {
            return Statuses.Any() ? Statuses.Max(s => s.ID) + 1 : 1;
        }

        private bool CanDeleteStatus(object parameter)
        {
            return SelectedStatus != null;
        }

        private void DeleteStatus(object parameter)
        {
            if (SelectedStatus != null)
            {
                var statusToDelete = _context.Status.Find(SelectedStatus.ID);
                if (statusToDelete != null)
                {
                    // Проверяем связи
                    if (statusToDelete.Order.Any())
                    {
                        System.Windows.MessageBox.Show("Нельзя удалить статус, так как он связан с заказами.");
                        return;
                    }

                    _context.Status.Remove(statusToDelete);
                }
                Statuses.Remove(SelectedStatus);
                SelectedStatus = null;
            }
        }

        private void SaveStatuses(object parameter)
        {
            try
            {
                _context.SaveChanges();
                System.Windows.MessageBox.Show("Статусы успешно сохранены.");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}