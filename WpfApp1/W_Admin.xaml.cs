using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1.classes_bd;
using WpfApp1.models;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для W_Admin.xaml
    /// </summary>
    public partial class W_Admin : Window
    {
        public MainViewModel _viewModel;

        public W_Admin()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            //DataContext = new MainViewModel();
            DataContext = _viewModel;
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);

            // Освобождаем ресурсы
            _viewModel.ProductVM?.Dispose();
            _viewModel.DishVM?.Dispose();
            _viewModel.TableVM?.Dispose();
            _viewModel.DishProdVM?.Dispose();

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Освобождаем ресурсы ViewModels
            _viewModel.ProductVM?.Dispose();
            _viewModel.DishVM?.Dispose();
            _viewModel.DishProdVM?.Dispose();
            _viewModel.TableVM?.Dispose();

            base.OnClosing(e);
        }
    }
}
