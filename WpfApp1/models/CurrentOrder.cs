using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class CurrentOrder : INotifyPropertyChanged
    {
        private ObservableCollection<OrderItem> _items = new ObservableCollection<OrderItem>();
        private int? _tempOrderId;
        private bool _isOrderCreated = false;

        public ObservableCollection<OrderItem> Items
        {
            get => _items;
            set
            {
                _items = value ?? new ObservableCollection<OrderItem>();
                OnPropertyChanged(nameof(Items));
            }
        }

        public decimal TotalPrice => Items?.Sum(item => item?.TotalPrice ?? 0) ?? 0;
        public int TotalItems => Items?.Sum(item => item?.Quantity ?? 0) ?? 0;

        public int? TempOrderId
        {
            get => _tempOrderId;
            set
            {
                _tempOrderId = value;
                OnPropertyChanged(nameof(TempOrderId));
            }
        }

        public bool IsOrderCreated
        {
            get => _isOrderCreated;
            set
            {
                _isOrderCreated = value;
                OnPropertyChanged(nameof(IsOrderCreated));
            }
        }

        public void AddItem(Dish dish, int quantity)
        {
            if (dish == null)
                throw new ArgumentNullException(nameof(dish), "Блюдо не может быть null");

            if (quantity <= 0)
                throw new ArgumentException("Количество должно быть больше 0", nameof(quantity));

            // Инициализируем коллекцию, если она null
            if (_items == null)
                _items = new ObservableCollection<OrderItem>();

            // Проверяем, есть ли уже такое блюдо в заказе
            var existingItem = _items.FirstOrDefault(item => item?.Dish?.ID == dish.ID);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.UpdateTotalPrice();
            }
            else
            {
                var newItem = new OrderItem
                {
                    Dish = dish,
                    Quantity = quantity,
                    Price = dish.Price ?? 0
                };
                newItem.UpdateTotalPrice();
                _items.Add(newItem);
            }

            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TotalItems));
        }

        public void RemoveItem(int dishId)
        {
            var item = _items?.FirstOrDefault(i => i?.Dish?.ID == dishId);
            if (item != null)
            {
                _items.Remove(item);
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(TotalItems));
            }
        }

        public void UpdateItemQuantity(int dishId, int quantity)
        {
            var item = _items?.FirstOrDefault(i => i?.Dish?.ID == dishId);
            if (item != null)
            {
                item.Quantity = quantity;
                item.UpdateTotalPrice();
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(TotalItems));
            }
        }

        public void Clear()
        {
            _items?.Clear();
            TempOrderId = null;
            IsOrderCreated = false;
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TotalItems));
        }

        public bool HasItems => _items?.Any() == true;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OrderItem : INotifyPropertyChanged
    {
        private Dish _dish;
        private int _quantity;
        private decimal _price;
        private decimal _totalPrice;

        public Dish Dish
        {
            get => _dish;
            set
            {
                _dish = value ?? throw new ArgumentNullException(nameof(Dish));
                OnPropertyChanged(nameof(Dish));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value <= 0) throw new ArgumentException("Количество должно быть больше 0");
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                UpdateTotalPrice();
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                if (value < 0) throw new ArgumentException("Цена не может быть отрицательной");
                _price = value;
                OnPropertyChanged(nameof(Price));
                UpdateTotalPrice();
            }
        }

        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                _totalPrice = value;
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public void UpdateTotalPrice()
        {
            TotalPrice = Price * Quantity;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}