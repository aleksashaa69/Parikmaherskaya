using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;
using Npgsql;
using System.IO;
using System.Runtime.InteropServices;
using Word = Microsoft.Office.Interop.Word;

namespace Parikmaherskaya
{
    public class ReportGenerator
    {
        private readonly string connString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=22ip22";

        public void GenerateReport()
        {
            Word.Application? wordApp = null;
            Word.Document? doc = null;

            try
            {
                var data = LoadIncomeData();

                string projectPath = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(projectPath, "Отчет_парикмахерская.docx");

                wordApp = new Word.Application();
                wordApp.Visible = true;

                doc = wordApp.Documents.Add();

                Word.Paragraph title = doc.Content.Paragraphs.Add();
                title.Range.Text = "Отчёт по доходам парикмахерской";
                title.Range.Font.Size = 16;
                title.Range.Font.Bold = 1;
                title.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                title.Range.InsertParagraphAfter();

                Word.Paragraph tablePara = doc.Content.Paragraphs.Add();
                Word.Table table = doc.Tables.Add(tablePara.Range, data.Count + 1, 3);
                table.Borders.Enable = 1;

                table.Cell(1, 1).Range.Text = "Услуга";
                table.Cell(1, 2).Range.Text = "Количество записей";
                table.Cell(1, 3).Range.Text = "Доход, ₽";

                table.Rows[1].Range.Font.Bold = 1;

                int row = 2;
                int totalRecords = 0;
                int totalIncome = 0;

                foreach (var item in data)
                {
                    table.Cell(row, 1).Range.Text = item.Service;
                    table.Cell(row, 2).Range.Text = item.Count.ToString();
                    table.Cell(row, 3).Range.Text = item.Income.ToString("N0") + " ₽";

                    totalRecords += item.Count;
                    totalIncome += item.Income;
                    row++;
                }

                table.Rows.Add();
                table.Cell(row, 1).Range.Text = "ИТОГО";
                table.Cell(row, 2).Range.Text = totalRecords.ToString();
                table.Cell(row, 3).Range.Text = totalIncome.ToString("N0") + " ₽";
                table.Rows[row].Range.Font.Bold = 1;

                doc.Content.InsertParagraphAfter();

                Word.Paragraph text = doc.Content.Paragraphs.Add();
                text.Range.Text = $"За период зафиксировано {totalRecords} записей на общую сумму {totalIncome:N0} рублей.";
                text.Range.InsertParagraphAfter();

                doc.SaveAs2(filePath);

                System.Windows.MessageBox.Show($"Отчёт создан в папке проекта:\n{filePath}\nWord открыт — проверь документ!", "Успех");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка создания отчёта:\n" + ex.Message, "Ошибка");
            }
        }

        private List<(string Service, int Count, int Income)> LoadIncomeData()
        {
            var list = new List<(string, int, int)>();

            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            string sql = @"
                SELECT u.naimenovanie, COUNT(z.id_zapisi), COALESCE(SUM(o.summa), 0)
                FROM public.usluga u
                LEFT JOIN public.zapis_usluga zu ON u.id_uslugi = zu.id_uslugi
                LEFT JOIN public.zapis z ON zu.id_zapisi = z.id_zapisi
                LEFT JOIN public.oplata o ON z.id_zapisi = o.id_zapisi
                GROUP BY u.naimenovanie
                ORDER BY COALESCE(SUM(o.summa), 0) DESC";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add((reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2)));
            }

            return list;
        }
    }
}