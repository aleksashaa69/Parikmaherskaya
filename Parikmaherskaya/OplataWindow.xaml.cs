using Npgsql;
using System;
using System.Collections.Generic;
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

namespace Parikmaherskaya
{
    public partial class OplataWindow : Window
    {
        private readonly string connString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=22ip22";

        private List<OplataView> AllOplaty { get; set; } = new();
        public List<OplataView> Oplaty { get; set; } = new();

        public OplataWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadOplaty();
        }

        private void LoadOplaty()
        {
            try
            {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();

                string sql = @"
                    SELECT o.id_oplaty, o.summa, o.data, o.nomer_cheka,
                           k.familiya || ' ' || k.imya || COALESCE(' ' || k.otchestvo, '') as klient,
                           so.naimenovanie as status, sp.naimenovanie as sposob
                    FROM public.oplata o
                    JOIN public.zapis z ON o.id_zapisi = z.id_zapisi
                    JOIN public.klient k ON z.id_klienta = k.id_klienta
                    JOIN public.status_oplaty so ON o.id_statusa_oplaty = so.id_statusa_oplaty
                    JOIN public.sposob_oplaty sp ON o.id_sposoba_oplaty = sp.id_sposoba_oplaty
                    ORDER BY o.data DESC";

                using var cmd = new NpgsqlCommand(sql, conn);
                using var r = cmd.ExecuteReader();

                AllOplaty.Clear();
                while (r.Read())
                {
                    AllOplaty.Add(new OplataView
                    {
                        IdOplaty = r.GetInt32(0),
                        Summa = r.GetInt32(1),
                        Data = r.GetDateTime(2),
                        NomerCheka = r.GetString(3),
                        KlientFIO = r.GetString(4),
                        Status = r.GetString(5),
                        Sposob = r.GetString(6)
                    });
                }

                Oplaty = new List<OplataView>(AllOplaty);
                OplataDataGrid.ItemsSource = Oplaty;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void Filter_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            string search = SearchBox?.Text.Trim().ToLower() ?? "";
            string filter = (StatusFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все оплаты";

            var result = AllOplaty.AsEnumerable();

            if (!string.IsNullOrEmpty(search))
                result = result.Where(o => o.KlientFIO.ToLower().Contains(search) || o.NomerCheka.ToLower().Contains(search));

            if (filter != "Все оплаты")
                result = result.Where(o => o.Status == filter);

            Oplaty = result.ToList();
            OplataDataGrid.ItemsSource = null;
            OplataDataGrid.ItemsSource = Oplaty;
        }
    }
}