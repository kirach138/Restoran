using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.classes_bd;

namespace WpfApp1.helpers_classes
{
    public class ProductValidator
    {
        public static string Validate(Product product)
        {
            if (product == null)
                return "Продукт не может быть пустым";

            if (string.IsNullOrWhiteSpace(product.Name))
                return "Название продукта не может быть пустым";

            if (product.Name.Length > 50)
                return "Название продукта не может превышать 50 символов";

            if (product.Cost < 0)
                return "Цена не может быть отрицательной";

            if (product.Count < 0)
                return "Количество не может быть отрицательным";

            if (product.Measure != null && product.Measure.Length > 50)
                return "Единица измерения не может превышать 50 символов";

            return null; // Валидация пройдена
        }
    }
}
