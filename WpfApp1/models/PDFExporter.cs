using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using iTextSharp.text;
using iTextSharp.text.pdf;
using WpfApp1.models;

namespace WpfApp1.Exporters
{
    public class PdfExporter
    {
        public void ExportToPdf(List<PopularDishReport> reportData,
                               DateTime startDate,
                               DateTime endDate,
                               decimal totalRevenue,
                               int totalOrders,
                               string filePath)
        {
            if (reportData == null || !reportData.Any())
                throw new ArgumentException("Нет данных для экспорта");

            Document document = new Document(PageSize.A4.Rotate(), 20, 20, 30, 30);

            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    PdfWriter writer = PdfWriter.GetInstance(document, stream);

                    // Добавляем обработчик событий для создания колонтитулов
                    writer.PageEvent = new PdfPageEventHandler();

                    document.Open();

                    // Добавляем заголовок
                    AddHeader(document, startDate, endDate);

                    // Добавляем сводную информацию
                    AddSummaryInfo(document, totalRevenue, totalOrders);

                    // Добавляем таблицу с данными
                    AddReportTable(document, reportData);

                    // Добавляем подвал
                    AddFooter(document);

                    document.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании PDF: {ex.Message}", ex);
            }
        }

        private void AddHeader(Document document, DateTime startDate, DateTime endDate)
        {
            // Шрифты
            BaseFont baseFont = BaseFont.CreateFont(
                "C:\\Users\\Huawei\\source\\repos\\RestoranK\\timesnrcyrmt.ttf",
                BaseFont.IDENTITY_H,
                BaseFont.NOT_EMBEDDED);

            Font headerFont = new Font(baseFont, 16, Font.BOLD);
            Font subHeaderFont = new Font(baseFont, 12, Font.BOLD);

            // Заголовок
            Paragraph header = new Paragraph("Отчет по популярным блюдам", headerFont);
            header.Alignment = Element.ALIGN_CENTER;
            header.SpacingAfter = 20f;
            document.Add(header);

            // Период отчета
            Paragraph period = new Paragraph(
                $"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}",
                subHeaderFont);
            period.Alignment = Element.ALIGN_CENTER;
            period.SpacingAfter = 15f;
            document.Add(period);

            // Дата формирования
            Paragraph generationDate = new Paragraph(
                $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}",
                new Font(baseFont, 10));
            generationDate.Alignment = Element.ALIGN_CENTER;
            generationDate.SpacingAfter = 25f;
            document.Add(generationDate);
        }

        private void AddSummaryInfo(Document document, decimal totalRevenue, int totalOrders)
        {
            BaseFont baseFont = BaseFont.CreateFont(
                "C:\\Users\\Huawei\\source\\repos\\RestoranK\\timesnrcyrmt.ttf",
                BaseFont.IDENTITY_H,
                BaseFont.NOT_EMBEDDED);

            Font summaryFont = new Font(baseFont, 11, Font.BOLD);
            Font listFont = new Font(baseFont, 10);

            // Сводная информация
            Paragraph summary = new Paragraph("Сводная информация:", summaryFont);
            summary.SpacingAfter = 10f;
            document.Add(summary);

            // Информация в виде списка
            iTextSharp.text.List summaryList = new iTextSharp.text.List(iTextSharp.text.List.UNORDERED);
            summaryList.IndentationLeft = 15f;
            summaryList.Add(new ListItem($"Общая выручка: {totalRevenue:C2}", listFont));
            summaryList.Add(new ListItem($"Общее количество заказов: {totalOrders}", listFont));

            // Добавляем промежуток после списка с помощью пустого параграфа
            document.Add(summaryList);

            // Пустой параграф для создания отступа
            Paragraph spacing = new Paragraph();
            spacing.SpacingAfter = 20f;
            document.Add(spacing);
        }

        private void AddReportTable(Document document, List<PopularDishReport> reportData)
        {
            BaseFont baseFont = BaseFont.CreateFont(
                "C:\\Users\\Huawei\\source\\repos\\RestoranK\\timesnrcyrmt.ttf",
                BaseFont.IDENTITY_H,
                BaseFont.NOT_EMBEDDED);

            Font tableHeaderFont = new Font(baseFont, 10, Font.BOLD);
            Font tableCellFont = new Font(baseFont, 9);

            // Создаем таблицу
            PdfPTable table = new PdfPTable(8); // 8 колонок
            table.WidthPercentage = 100;
            table.SpacingBefore = 10f;
            table.SpacingAfter = 30f;

            // Устанавливаем ширины колонок
            float[] columnWidths = { 0.5f, 3f, 1f, 1f, 1.2f, 1f, 1f, 1f };
            table.SetWidths(columnWidths);

            // Заголовки таблицы
            AddTableHeader(table, tableHeaderFont);

            // Данные таблицы
            AddTableData(table, reportData, tableCellFont);

            document.Add(table);
        }

        private void AddTableHeader(PdfPTable table, Font font)
        {
            string[] headers =
            {
                "№",
                "Название блюда",
                "Цена",
                "Кол-во",
                "Выручка",
                "Заказов",
                "Ср. в заказе",
                "% от общего"
            };

            foreach (string header in headers)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, font));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5;
                cell.BackgroundColor = new BaseColor(240, 240, 240);
                table.AddCell(cell);
            }
        }

        private void AddTableData(PdfPTable table, List<PopularDishReport> data, Font font)
        {
            int rowNumber = 1;

            foreach (var item in data)
            {
                // Номер строки
                table.AddCell(new PdfPCell(new Phrase(rowNumber++.ToString(), font))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                });

                // Название блюда
                table.AddCell(new PdfPCell(new Phrase(item.DishName, font))
                {
                    Padding = 5
                });

                // Цена
                table.AddCell(new PdfPCell(new Phrase(item.Price.ToString("C2"), font))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                });

                // Количество
                table.AddCell(new PdfPCell(new Phrase(item.TotalQuantity.ToString(), font))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                });

                // Выручка
                table.AddCell(new PdfPCell(new Phrase(item.TotalRevenue.ToString("C2"), font))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                });

                // Заказов
                table.AddCell(new PdfPCell(new Phrase(item.OrderCount.ToString(), font))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                });

                // Среднее в заказе
                table.AddCell(new PdfPCell(new Phrase(item.AveragePerOrder.ToString("F2"), font))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                });

                // Процент
                table.AddCell(new PdfPCell(new Phrase(item.FormattedPercentage, font))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                });
            }
        }

        private void AddFooter(Document document)
        {
            BaseFont baseFont = BaseFont.CreateFont(
                "C:\\Users\\Huawei\\source\\repos\\RestoranK\\timesnrcyrmt.ttf",
                BaseFont.IDENTITY_H,
                BaseFont.NOT_EMBEDDED);

            Font footerFont = new Font(baseFont, 9, Font.ITALIC);

            Paragraph footer = new Paragraph(
                " ",
                footerFont);
            footer.Alignment = Element.ALIGN_CENTER;
            document.Add(footer);
        }

        // Класс для обработки событий страницы (колонтитулы)
        private class PdfPageEventHandler : PdfPageEventHelper
        {
            private PdfTemplate totalPages;
            private BaseFont baseFont;

            public override void OnOpenDocument(PdfWriter writer, Document document)
            {
                baseFont = BaseFont.CreateFont(
                    "c:\\windows\\fonts\\arial.ttf",
                    BaseFont.IDENTITY_H,
                    BaseFont.NOT_EMBEDDED);

                totalPages = writer.DirectContent.CreateTemplate(30, 16);
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                PdfPTable footer = new PdfPTable(3);
                footer.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                footer.DefaultCell.Border = Rectangle.NO_BORDER;

                // Левая часть - дата
                footer.AddCell(new PdfPCell(new Phrase(
                    DateTime.Now.ToString("dd.MM.yyyy"),
                    new Font(baseFont, 8)))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });

                // Центр - заголовок
                footer.AddCell(new PdfPCell(new Phrase(
                    "Отчет по популярным блюдам",
                    new Font(baseFont, 8, Font.BOLD)))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });

                // Правая часть - нумерация страниц
                string pageText = $"Страница {writer.PageNumber} из ";
                footer.AddCell(new PdfPCell(
                    new Phrase(pageText, new Font(baseFont, 8)))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });

                // Позиционируем таблицу внизу страницы
                footer.WriteSelectedRows(
                    0, -1,
                    document.LeftMargin,
                    document.BottomMargin,
                    writer.DirectContent);

                // Добавляем общее количество страниц
                ColumnText.ShowTextAligned(
                    writer.DirectContent,
                    Element.ALIGN_RIGHT,
                    new Phrase(totalPages.ToString(), new Font(baseFont, 8)),
                    document.RightMargin,
                    document.BottomMargin,
                    0);
            }

            public override void OnCloseDocument(PdfWriter writer, Document document)
            {
                ColumnText.ShowTextAligned(
                    totalPages,
                    Element.ALIGN_LEFT,
                    new Phrase(writer.PageNumber.ToString(), new Font(baseFont, 8)),
                    0, 0, 0);
            }
        }
    }
}