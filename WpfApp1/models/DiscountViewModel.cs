using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using WpfApp1.classes_bd;

namespace WpfApp1.models
{
    public class DiscountViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private Discount _selectedDiscount;

        public ObservableCollection<Discount> Discounts { get; set; }
        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }

        public Discount SelectedDiscount
        {
            get => _selectedDiscount;
            set
            {
                _selectedDiscount = value;
                OnPropertyChanged(nameof(SelectedDiscount));
            }
        }

        public DiscountViewModel(Model_R context)
        {
            _context = context ?? new Model_R();
            Discounts = new ObservableCollection<Discount>();
            LoadDiscounts();

            AddCommand = new RelayCommand(AddDiscount);
            DeleteCommand = new RelayCommand(DeleteDiscount, CanDeleteDiscount);
            SaveCommand = new RelayCommand(SaveDiscounts);
        }

        public void LoadDiscounts()
        {
            Discounts.Clear();
            foreach (var discount in _context.Discount.ToList())
            {
                Discounts.Add(discount);
            }
        }

        private void AddDiscount(object parameter)
        {
            var newDiscount = new Discount
            {
                ID = GetNextId(),
                Name = "Новая скидка",
                Value = 0
            };

            Discounts.Add(newDiscount);
            _context.Discount.Add(newDiscount);
            SelectedDiscount = newDiscount;
        }

        private int GetNextId()
        {
            return Discounts.Any() ? Discounts.Max(d => d.ID) + 1 : 1;
        }

        private bool CanDeleteDiscount(object parameter)
        {
            return SelectedDiscount != null;
        }

        private void DeleteDiscount(object parameter)
        {
            if (SelectedDiscount != null)
            {
                var discountToDelete = _context.Discount.Find(SelectedDiscount.ID);
                if (discountToDelete != null)
                {
                    // Проверяем связи
                    if (discountToDelete.Ord_dish.Any())
                    {
                        System.Windows.MessageBox.Show("Нельзя удалить скидку, так как она связана с блюдами заказов.");
                        return;
                    }

                    _context.Discount.Remove(discountToDelete);
                }
                Discounts.Remove(SelectedDiscount);
                SelectedDiscount = null;
            }
        }

        private void SaveDiscounts(object parameter)
        {
            try
            {
                _context.SaveChanges();
                System.Windows.MessageBox.Show("Скидки успешно сохранены.");
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