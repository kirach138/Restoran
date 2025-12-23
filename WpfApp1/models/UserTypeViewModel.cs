using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class UserTypeViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private User_type _selectedUserType;

        public ObservableCollection<User_type> UserTypes { get; set; }
        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }

        public User_type SelectedUserType
        {
            get => _selectedUserType;
            set
            {
                _selectedUserType = value;
                OnPropertyChanged(nameof(SelectedUserType));
            }
        }

        public UserTypeViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            UserTypes = new ObservableCollection<User_type>();
            LoadUserTypes();

            AddCommand = new RelayCommand(AddUserType);
            DeleteCommand = new RelayCommand(DeleteUserType, CanDeleteUserType);
            SaveCommand = new RelayCommand(SaveUserTypes);
        }

        public void LoadUserTypes()
        {
            UserTypes.Clear();
            foreach (var type in _context.User_type.ToList())
            {
                UserTypes.Add(type);
            }
        }

        private void AddUserType(object parameter)
        {
            var newType = new User_type
            {
                ID = GetNextId(),
                Name = "Новый тип"
            };
            UserTypes.Add(newType);
            _context.User_type.Add(newType);
            SelectedUserType = newType;
        }

        private int GetNextId()
        {
            return UserTypes.Any() ? UserTypes.Max(t => t.ID) + 1 : 1;
        }

        private bool CanDeleteUserType(object parameter)
        {
            return SelectedUserType != null;
        }

        private void DeleteUserType(object parameter)
        {
            if (SelectedUserType != null)
            {
                var typeToDelete = _context.User_type.Find(SelectedUserType.ID);
                if (typeToDelete != null)
                {
                    // Проверяем, есть ли пользователи с этим типом
                    if (typeToDelete.User.Any())
                    {
                        // Можно показать сообщение пользователю
                        System.Windows.MessageBox.Show("Нельзя удалить тип пользователя, так как есть пользователи с этим типом.");
                        return;
                    }

                    _context.User_type.Remove(typeToDelete);
                }
                UserTypes.Remove(SelectedUserType);
                SelectedUserType = null;
            }
        }

        private void SaveUserTypes(object parameter)
        {
            try
            {
                _context.SaveChanges();
                System.Windows.MessageBox.Show("Типы пользователей успешно сохранены.");
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