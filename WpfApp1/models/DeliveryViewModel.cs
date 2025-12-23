using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class DeliveryViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Delivery _selectedDelivery;
        private ObservableCollection<User> _availableUsers;

        public ObservableCollection<Delivery> Deliveries { get; set; }
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

        public Delivery SelectedDelivery
        {
            get => _selectedDelivery;
            set
            {
                _selectedDelivery = value;
                OnPropertyChanged(nameof(SelectedDelivery));
            }
        }

        public DeliveryViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            Deliveries = new ObservableCollection<Delivery>();
            AvailableUsers = new ObservableCollection<User>();
            LoadData();

            AddCommand = new RelayCommand(AddDelivery);
            DeleteCommand = new RelayCommand(DeleteDelivery, CanDeleteDelivery);
            SaveCommand = new RelayCommand(SaveDeliveries);
        }

        public void LoadData()
        {
            Deliveries.Clear();
            foreach (var delivery in _context.Delivery.Include(d => d.User).ToList())
            {
                Deliveries.Add(delivery);
            }

            AvailableUsers.Clear();
            foreach (var user in _context.User.ToList())
            {
                AvailableUsers.Add(user);
            }
        }

        private void AddDelivery(object parameter)
        {
            var newDelivery = new Delivery
            {
                ID = GetNextId(),
                Userid = AvailableUsers.FirstOrDefault()?.ID
            };

            if (newDelivery.Userid.HasValue)
            {
                newDelivery.User = AvailableUsers.FirstOrDefault(u => u.ID == newDelivery.Userid.Value);
            }

            Deliveries.Add(newDelivery);
            _context.Delivery.Add(newDelivery);
            SelectedDelivery = newDelivery;
        }

        private int GetNextId()
        {
            return Deliveries.Any() ? Deliveries.Max(d => d.ID) + 1 : 1;
        }

        private bool CanDeleteDelivery(object parameter)
        {
            return SelectedDelivery != null;
        }

        private void DeleteDelivery(object parameter)
        {
            if (SelectedDelivery != null)
            {
                var deliveryToDelete = _context.Delivery.Find(SelectedDelivery.ID);
                if (deliveryToDelete != null)
                {
                    // Проверяем связи
                    if (deliveryToDelete.Order.Any())
                    {
                        System.Windows.MessageBox.Show("Нельзя удалить доставщика, так как он связан с заказами.");
                        return;
                    }

                    _context.Delivery.Remove(deliveryToDelete);
                }
                Deliveries.Remove(SelectedDelivery);
                SelectedDelivery = null;
            }
        }

        private void SaveDeliveries(object parameter)
        {
            try
            {
                _context.SaveChanges();
                System.Windows.MessageBox.Show("Доставщики успешно сохранены.");
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