using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class WaiterViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Waiter _selectedWaiter;
        private ObservableCollection<User> _availableUsers;

        public ObservableCollection<Waiter> Waiters { get; set; }
        public ObservableCollection<User> AvailableUsers
        {
            get => _availableUsers;
            set
            {
                _availableUsers = value;
                OnPropertyChanged(nameof(AvailableUsers));
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }

        public Waiter SelectedWaiter
        {
            get => _selectedWaiter;
            set
            {
                _selectedWaiter = value;
                OnPropertyChanged(nameof(SelectedWaiter));
            }
        }

        public WaiterViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            Waiters = new ObservableCollection<Waiter>();
            AvailableUsers = new ObservableCollection<User>();
            LoadData();

            AddCommand = new RelayCommand(AddWaiter);
            DeleteCommand = new RelayCommand(DeleteWaiter, CanDeleteWaiter);
            SaveCommand = new RelayCommand(SaveWaiters);
        }

        public void LoadData()
        {
            Waiters.Clear();
            foreach (var waiter in _context.Waiter.Include(w => w.User).ToList())
            {
                Waiters.Add(waiter);
            }

            AvailableUsers.Clear();
            foreach (var user in _context.User.ToList())
            {
                AvailableUsers.Add(user);
            }
        }

        private void AddWaiter(object parameter)
        {
            var newWaiter = new Waiter
            {
                ID = GetNextId(),
                Userid = AvailableUsers.FirstOrDefault()?.ID
            };

            if (newWaiter.Userid.HasValue)
            {
                newWaiter.User = AvailableUsers.FirstOrDefault(u => u.ID == newWaiter.Userid.Value);
            }

            Waiters.Add(newWaiter);
            _context.Waiter.Add(newWaiter);
            SelectedWaiter = newWaiter;
        }

        private int GetNextId()
        {
            return Waiters.Any() ? Waiters.Max(w => w.ID) + 1 : 1;
        }

        private bool CanDeleteWaiter(object parameter)
        {
            return SelectedWaiter != null;
        }

        private void DeleteWaiter(object parameter)
        {
            if (SelectedWaiter != null)
            {
                var waiterToDelete = _context.Waiter.Find(SelectedWaiter.ID);
                if (waiterToDelete != null)
                {
                    // Проверяем связи
                    if (waiterToDelete.Order.Any())
                    {
                        System.Windows.MessageBox.Show("Нельзя удалить официанта, так как он связан с заказами.");
                        return;
                    }

                    _context.Waiter.Remove(waiterToDelete);
                }
                Waiters.Remove(SelectedWaiter);
                SelectedWaiter = null;
            }
        }

        private void SaveWaiters(object parameter)
        {
            try
            {
                _context.SaveChanges();
                System.Windows.MessageBox.Show("Официанты успешно сохранены.");
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