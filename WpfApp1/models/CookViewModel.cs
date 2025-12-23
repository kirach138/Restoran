using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class CookViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Cook _selectedCook;
        private ObservableCollection<User> _availableUsers;

        public ObservableCollection<Cook> Cooks { get; set; }
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

        public Cook SelectedCook
        {
            get => _selectedCook;
            set
            {
                _selectedCook = value;
                OnPropertyChanged(nameof(SelectedCook));
            }
        }

        public CookViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            Cooks = new ObservableCollection<Cook>();
            AvailableUsers = new ObservableCollection<User>();
            LoadData();

            AddCommand = new RelayCommand(AddCook);
            DeleteCommand = new RelayCommand(DeleteCook, CanDeleteCook);
            SaveCommand = new RelayCommand(SaveCooks);
        }

        public void LoadData()
        {
            Cooks.Clear();
            foreach (var cook in _context.Cook.Include(c => c.User).ToList())
            {
                Cooks.Add(cook);
            }

            AvailableUsers.Clear();
            foreach (var user in _context.User.ToList())
            {
                AvailableUsers.Add(user);
            }
        }

        private void AddCook(object parameter)
        {
            var newCook = new Cook
            {
                ID = GetNextId(),
                Userid = AvailableUsers.FirstOrDefault()?.ID,
                Specialization = "Повар"
            };

            if (newCook.Userid.HasValue)
            {
                newCook.User = AvailableUsers.FirstOrDefault(u => u.ID == newCook.Userid.Value);
            }

            Cooks.Add(newCook);
            _context.Cook.Add(newCook);
            SelectedCook = newCook;
        }

        private int GetNextId()
        {
            return Cooks.Any() ? Cooks.Max(c => c.ID) + 1 : 1;
        }

        private bool CanDeleteCook(object parameter)
        {
            return SelectedCook != null;
        }

        private void DeleteCook(object parameter)
        {
            if (SelectedCook != null)
            {
                var cookToDelete = _context.Cook.Find(SelectedCook.ID);
                if (cookToDelete != null)
                {
                    // Проверяем связи
                    if (cookToDelete.Ord_dish.Any())
                    {
                        System.Windows.MessageBox.Show("Нельзя удалить повара, так как он связан с блюдами заказов.");
                        return;
                    }

                    _context.Cook.Remove(cookToDelete);
                }
                Cooks.Remove(SelectedCook);
                SelectedCook = null;
            }
        }

        private void SaveCooks(object parameter)
        {
            try
            {
                _context.SaveChanges();
                System.Windows.MessageBox.Show("Повара успешно сохранены.");
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