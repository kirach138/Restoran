using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Client _selectedClient;
        private ObservableCollection<User> _availableUsers;

        public ObservableCollection<Client> Clients { get; set; }
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

        public Client SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
            }
        }

        public ClientViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            Clients = new ObservableCollection<Client>();
            AvailableUsers = new ObservableCollection<User>();
            LoadData();

            AddCommand = new RelayCommand(AddClient);
            DeleteCommand = new RelayCommand(DeleteClient, CanDeleteClient);
            SaveCommand = new RelayCommand(SaveClients);
        }

        public void LoadData()
        {
            Clients.Clear();
            foreach (var client in _context.Client.Include(c => c.User).ToList())
            {
                Clients.Add(client);
            }

            AvailableUsers.Clear();
            foreach (var user in _context.User.ToList())
            {
                AvailableUsers.Add(user);
            }
        }

        private void AddClient(object parameter)
        {
            var newClient = new Client
            {
                ID = GetNextId(),
                Count = 0,
                Userid = AvailableUsers.FirstOrDefault()?.ID
            };

            if (newClient.Userid.HasValue)
            {
                newClient.User = AvailableUsers.FirstOrDefault(u => u.ID == newClient.Userid.Value);
            }

            Clients.Add(newClient);
            _context.Client.Add(newClient);
            SelectedClient = newClient;
        }

        private int GetNextId()
        {
            return Clients.Any() ? Clients.Max(c => c.ID) + 1 : 1;
        }

        private bool CanDeleteClient(object parameter)
        {
            return SelectedClient != null;
        }

        private void DeleteClient(object parameter)
        {
            if (SelectedClient != null)
            {
                var clientToDelete = _context.Client.Find(SelectedClient.ID);
                if (clientToDelete != null)
                {
                    // Проверяем связи
                    if (clientToDelete.Booking.Any() || clientToDelete.Order.Any())
                    {
                        System.Windows.MessageBox.Show("Нельзя удалить клиента, так как он связан с бронированиями или заказами.");
                        return;
                    }

                    _context.Client.Remove(clientToDelete);
                }
                Clients.Remove(SelectedClient);
                SelectedClient = null;
            }
        }

        private void SaveClients(object parameter)
        {
            try
            {
                _context.SaveChanges();
                System.Windows.MessageBox.Show("Клиенты успешно сохранены.");
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