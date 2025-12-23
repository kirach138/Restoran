using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private User _selectedUser;
        private ObservableCollection<User_type> _userTypes;

        public ObservableCollection<User> Users { get; set; }
        public ObservableCollection<User_type> UserTypes
        {
            get => _userTypes;
            set
            {
                _userTypes = value;
                OnPropertyChanged(nameof(UserTypes));
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }

        public UserViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            Users = new ObservableCollection<User>();
            UserTypes = new ObservableCollection<User_type>();
            LoadData();

            AddCommand = new RelayCommand(AddUser);
            DeleteCommand = new RelayCommand(DeleteUser, CanDeleteUser);
            SaveCommand = new RelayCommand(SaveUsers);
        }

        public void LoadData()
        {
            Users.Clear();
            foreach (var user in _context.User.Include(u => u.User_type).ToList())
            {
                Users.Add(user);
            }

            UserTypes.Clear();
            foreach (var type in _context.User_type.ToList())
            {
                UserTypes.Add(type);
            }
        }

        private void AddUser(object parameter)
        {
            var newUser = new User
            {
                ID = GetNextId(),
                Login = "Новый логин",
                Name = "Новое имя",
                Type = 1, // Стандартный тип
                Password = "password",
                Phone = "0000000000"
            };

            // Устанавливаем связь с типом пользователя
            newUser.User_type = UserTypes.FirstOrDefault(t => t.ID == newUser.Type);

            Users.Add(newUser);
            _context.User.Add(newUser);
            SelectedUser = newUser;
        }

        private int GetNextId()
        {
            return Users.Any() ? Users.Max(u => u.ID) + 1 : 1;
        }

        private bool CanDeleteUser(object parameter)
        {
            return SelectedUser != null;
        }

        private void DeleteUser(object parameter)
        {
            if (SelectedUser != null)
            {
                var userToDelete = _context.User.Find(SelectedUser.ID);
                if (userToDelete != null)
                {
                    // Проверяем связи
                    if (userToDelete.Admin.Any() || userToDelete.Client.Any() ||
                        userToDelete.Cook.Any() || userToDelete.Delivery.Any() ||
                        userToDelete.Waiter.Any())
                    {
                        System.Windows.MessageBox.Show("Нельзя удалить пользователя, так как он связан с другими записями.");
                        return;
                    }

                    _context.User.Remove(userToDelete);
                }
                Users.Remove(SelectedUser);
                SelectedUser = null;
            }
        }

        private void SaveUsers(object parameter)
        {
            try
            {
                _context.SaveChanges();
                System.Windows.MessageBox.Show("Пользователи успешно сохранены.");
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