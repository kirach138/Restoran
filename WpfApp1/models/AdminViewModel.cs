using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Admin _selectedAdmin;
        private ObservableCollection<User> _availableUsers;

        public ObservableCollection<Admin> Admins { get; set; }
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

        public Admin SelectedAdmin
        {
            get => _selectedAdmin;
            set
            {
                _selectedAdmin = value;
                OnPropertyChanged(nameof(SelectedAdmin));
            }
        }

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!_disposed && disposing)
        //    {
        //        _context?.Dispose();
        //    }
        //    _disposed = true;
        //}

        public AdminViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            Admins = new ObservableCollection<Admin>();
            AvailableUsers = new ObservableCollection<User>();
            LoadData();

            AddCommand = new RelayCommand(AddAdmin);
            DeleteCommand = new RelayCommand(DeleteAdmin, CanDeleteAdmin);
            SaveCommand = new RelayCommand(SaveAdmins);
        }

        public void LoadData()
        {
            Admins.Clear();
            foreach (var admin in _context.Admin.Include(a => a.User).ToList())
            {
                Admins.Add(admin);
            }

            AvailableUsers.Clear();
            foreach (var user in _context.User.ToList())
            {
                AvailableUsers.Add(user);
            }
        }

        private void AddAdmin(object parameter)
        {
            var newAdmin = new Admin
            {
                ID = GetNextId(),
                Userid = AvailableUsers.FirstOrDefault()?.ID
            };

            if (newAdmin.Userid.HasValue)
            {
                newAdmin.User = AvailableUsers.FirstOrDefault(u => u.ID == newAdmin.Userid.Value);
            }

            Admins.Add(newAdmin);
            _context.Admin.Add(newAdmin);
            SelectedAdmin = newAdmin;
        }

        private int GetNextId()
        {
            return Admins.Any() ? Admins.Max(a => a.ID) + 1 : 1;
        }

        private bool CanDeleteAdmin(object parameter)
        {
            return SelectedAdmin != null;
        }

        private void DeleteAdmin(object parameter)
        {
            if (SelectedAdmin != null)
            {
                var adminToDelete = _context.Admin.Find(SelectedAdmin.ID);
                if (adminToDelete != null)
                {
                    _context.Admin.Remove(adminToDelete);
                }
                Admins.Remove(SelectedAdmin);
                SelectedAdmin = null;
            }
        }

        private void SaveAdmins(object parameter)
        {
            try
            {
                _context.SaveChanges();
                System.Windows.MessageBox.Show("Администраторы успешно сохранены.");
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