using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfApp1.classes_bd;
using WpfApp1.Exporters;

namespace WpfApp1.models
{
    public class ReportViewModel : INotifyPropertyChanged
    {
        private Model_R _context;
        private DateTime _startDate;
        private DateTime _endDate;
        private ObservableCollection<PopularDishReport> _popularDishes;
        private decimal _totalRevenue;
        private int _totalOrders;

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public ObservableCollection<PopularDishReport> PopularDishes
        {
            get => _popularDishes;
            set
            {
                _popularDishes = value;
                OnPropertyChanged(nameof(PopularDishes));
            }
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged(nameof(TotalRevenue));
            }
        }

        public int TotalOrders
        {
            get => _totalOrders;
            set
            {
                _totalOrders = value;
                OnPropertyChanged(nameof(TotalOrders));
            }
        }

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportToExcelCommand { get; }

        public ICommand ExportToPdfCommand { get; }

        public ReportViewModel(Model_R context = null)
        {
            _context = context ?? new Model_R();
            PopularDishes = new ObservableCollection<PopularDishReport>();

            // Устанавливаем диапазон дат по умолчанию (последний месяц)
            EndDate = DateTime.Today;
            StartDate = EndDate.AddMonths(-1);

            ExportToPdfCommand = new RelayCommand(ExportToPdf, CanExport);
            GenerateReportCommand = new RelayCommand(GenerateReport);
            ExportToExcelCommand = new RelayCommand(ExportToExcel, CanExport);
        }

        // Метод для генерации отчета
        public void GenerateReport(object parameter)
        {
            try
            {
                PopularDishes.Clear();

                // Вычисляем конечную дату (включительно до конца дня)
                var endDate = EndDate.Date.AddDays(1);

                // Получаем данные о популярных блюдах за указанный период
                var popularDishesData = _context.Ord_dish
                    .Include(od => od.Dish)
                    .Include(od => od.Order)
                    .Where(od => od.Order.Time >= StartDate && od.Order.Time < endDate)
                    .GroupBy(od => new { od.Dishid, od.Dish.Name, od.Dish.Price })
                    .Select(g => new
                    {
                        DishId = g.Key.Dishid,
                        DishName = g.Key.Name,
                        Price = g.Key.Price ?? 0,
                        TotalQuantity = g.Sum(od => od.Count ?? 0),
                        TotalRevenue = g.Sum(od => od.Cost ?? 0),
                        OrderCount = g.Select(od => od.Orderid).Distinct().Count()
                    })
                    .Where(x => x.TotalQuantity > 0)
                    .OrderByDescending(x => x.TotalQuantity)
                    .ToList();

                // Рассчитываем общее количество для вычисления процентов
                decimal totalQuantity = popularDishesData.Sum(x => x.TotalQuantity);

                // Преобразуем в коллекцию отчетов
                int rank = 1;
                foreach (var item in popularDishesData)
                {
                    decimal percentage = totalQuantity > 0 ? (item.TotalQuantity / (decimal)totalQuantity * 100) : 0;

                    PopularDishes.Add(new PopularDishReport
                    {
                        DishName = item.DishName ?? "Неизвестное блюдо",
                        Price = item.Price,
                        TotalQuantity = item.TotalQuantity,
                        TotalRevenue = item.TotalRevenue,
                        OrderCount = item.OrderCount,
                        AveragePerOrder = item.OrderCount > 0 ? item.TotalQuantity / (decimal)item.OrderCount : 0,
                        PopularityRank = rank++,
                        PercentageOfTotal = percentage
                    });
                }

                // Рассчитываем общую статистику
                TotalRevenue = popularDishesData.Sum(x => x.TotalRevenue);

                // Считаем количество уникальных заказов за период
                TotalOrders = _context.Order
                    .Where(o => o.Time >= StartDate && o.Time < endDate)
                    .Select(o => o.ID)
                    .Distinct()
                    .Count();

                OnPropertyChanged(nameof(PopularDishes));
                OnPropertyChanged(nameof(TotalRevenue));
                OnPropertyChanged(nameof(TotalOrders));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при генерации отчета: {ex.Message}\n{ex.InnerException?.Message}",
                    "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void ExportToPdf(object parameter)
        {
            try
            {
                if (!PopularDishes.Any())
                {
                    MessageBox.Show("Нет данных для экспорта. Сначала сгенерируйте отчет.",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Отчет_популярные_блюда_{DateTime.Now:yyyy-MM-dd_HHmm}",
                    DefaultExt = ".pdf",
                    Filter = "PDF файлы (.pdf)|*.pdf|Все файлы (*.*)|*.*"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var exporter = new PdfExporter();

                    exporter.ExportToPdf(
                        reportData: PopularDishes.ToList(),
                        startDate: StartDate,
                        endDate: EndDate,
                        totalRevenue: TotalRevenue,
                        totalOrders: TotalOrders,
                        filePath: saveDialog.FileName
                    );

                    MessageBox.Show($"Отчет успешно экспортирован в PDF!\nФайл: {saveDialog.FileName}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Опционально: открыть файл после сохранения
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        // Если не удалось открыть файл, игнорируем ошибку
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в PDF: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для экспорта в Excel (заглушка)
        private void ExportToExcel(object parameter)
        {
            try
            {
                if (!PopularDishes.Any())
                {
                    System.Windows.MessageBox.Show("Нет данных для экспорта. Сначала сгенерируйте отчет.",
                        "Информация", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    return;
                }

                // Здесь будет логика экспорта в Excel
                // Например, можно использовать библиотеку EPPlus или ClosedXML

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Отчет_популярные_блюда_{DateTime.Now:yyyy-MM-dd}",
                    DefaultExt = ".xlsx",
                    Filter = "Excel файлы (.xlsx)|*.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // TODO: Реализовать экспорт в Excel
                    System.Windows.MessageBox.Show($"Экспорт в Excel будет реализован. Файл: {saveDialog.FileName}",
                        "Информация", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}",
                    "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool CanExport(object parameter)
        {
            return PopularDishes != null && PopularDishes.Any();
        }

        // Метод для получения отчета по топ N блюд
        public List<PopularDishReport> GetTopPopularDishes(int topCount = 10)
        {
            return PopularDishes.Take(topCount).ToList();
        }

        // Метод для сброса отчета
        public void ClearReport()
        {
            PopularDishes.Clear();
            TotalRevenue = 0;
            TotalOrders = 0;
            OnPropertyChanged(nameof(PopularDishes));
            OnPropertyChanged(nameof(TotalRevenue));
            OnPropertyChanged(nameof(TotalOrders));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Класс для отображения данных отчета
    public class PopularDishReport : INotifyPropertyChanged
    {
        private string _dishName;
        private decimal _price;
        private int _totalQuantity;
        private decimal _totalRevenue;
        private int _orderCount;
        private decimal _averagePerOrder;

        public string DishName
        {
            get => _dishName;
            set
            {
                _dishName = value;
                OnPropertyChanged(nameof(DishName));
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(FormattedPrice));
            }
        }

        public string FormattedPrice => Price.ToString("C2");

        public int TotalQuantity
        {
            get => _totalQuantity;
            set
            {
                _totalQuantity = value;
                OnPropertyChanged(nameof(TotalQuantity));
            }
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged(nameof(TotalRevenue));
                OnPropertyChanged(nameof(FormattedRevenue));
            }
        }

        public string FormattedRevenue => TotalRevenue.ToString("C2");

        public int OrderCount
        {
            get => _orderCount;
            set
            {
                _orderCount = value;
                OnPropertyChanged(nameof(OrderCount));
            }
        }

        public decimal AveragePerOrder
        {
            get => _averagePerOrder;
            set
            {
                _averagePerOrder = value;
                OnPropertyChanged(nameof(AveragePerOrder));
                OnPropertyChanged(nameof(FormattedAverage));
            }
        }

        public string FormattedAverage => AveragePerOrder.ToString("F2");

        // Рейтинг популярности (вычисляемое свойство)
        public int PopularityRank { get; set; }

        // Процент от общего количества
        private decimal _percentageOfTotal;
        public decimal PercentageOfTotal
        {
            get => _percentageOfTotal;
            set
            {
                _percentageOfTotal = value;
                OnPropertyChanged(nameof(PercentageOfTotal));
                OnPropertyChanged(nameof(FormattedPercentage));
            }
        }

        public string FormattedPercentage => $"{PercentageOfTotal:F1}%";

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}