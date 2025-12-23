// W_Dish.xaml.cs
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.classes_bd;
using WpfApp1.models;

namespace WpfApp1
{
    public partial class W_Dish : Window
    {
        private Dish _selectedDish;
        private DishProdViewModel _dishProdVM;
        private int _quantity = 1;
        private MainViewModel _mainViewModel;

        public int SelectedQuantity => _quantity;
        public Dish SelectedDish => _selectedDish;

        public W_Dish(MainViewModel mainViewModel = null)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
            UpdateQuantityDisplay();
        }

        // Метод для загрузки информации о блюде
        public void LoadDishInfo(int dishId)
        {
            using (var context = new Model_R())
            {
                _selectedDish = context.Dish.FirstOrDefault(d => d.ID == dishId);

                if (_selectedDish != null)
                {
                    this.Title = _selectedDish.Name;
                    DishTitle.Text = _selectedDish.Name;
                    DescriptionText.Text = _selectedDish.Description ?? "Описание отсутствует";
                    PriceText.Text = _selectedDish.Price?.ToString("C") ?? "Цена не указана";

                    AvailabilityText.Text = (_selectedDish.Availability > 0) ?
                        "В наличии" : "Нет в наличии";

                    if (_selectedDish.Availability <= 0)
                    {
                        AddToOrderButton.IsEnabled = false;
                        PlusButton.IsEnabled = false;
                        MinusButton.IsEnabled = false;
                        AddToOrderButton.Content = "Нет в наличии";
                        AddToOrderButton.Background = System.Windows.Media.Brushes.Gray;
                    }

                    LoadDishComposition(dishId);
                }
            }
        }

        // Метод для загрузки состава блюда
        private void LoadDishComposition(int dishId)
        {
            using (var context = new Model_R())
            {
                var dishProducts = context.Dish_prod
                    .Where(dp => dp.Dishid == dishId)
                    .Include("Product")
                    .Include("Dish")
                    .ToList();

                ProductsList.ItemsSource = dishProducts;

                if (!dishProducts.Any())
                {
                    ProductsList.ItemsSource = new[]
                    {
                        new { Product = new Product { Name = "Состав не указан" }, Count = 0 }
                    };
                }
            }
        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            if (_quantity > 1)
            {
                _quantity--;
                UpdateQuantityDisplay();
            }
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDish.Availability == null || _quantity < _selectedDish.Availability)
            {
                _quantity++;
                UpdateQuantityDisplay();
            }
            else
            {
                MessageBox.Show("Недостаточно товара в наличии",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void UpdateQuantityDisplay()
        {
            QuantityText.Text = _quantity.ToString();
        }

        private void AddToOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDish != null)
            {
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}