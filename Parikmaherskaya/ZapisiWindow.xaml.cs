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
    public partial class ZapisiWindow : Window
    {
        private readonly string connString =
            "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=22ip22";

        private List<ZapisView> AllZapisi { get; set; } = new();
        private List<ZapisView> Zapisi { get; set; } = new();

        public ZapisiWindow()
        {
            InitializeComponent();
            Loaded += ZapisiWindow_Loaded;
        }

        private void ZapisiWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadZapisi();
        }

        private void LoadZapisi()
        {
            try
            {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();

                string sql = @"
                    SELECT
                        z.id_zapisi,
                        z.vremya_data,
                        k.familiya || ' ' || k.imya || COALESCE(' ' || k.otchestvo, '') as klient_fio,
                        s.familiya || ' ' || s.imya || COALESCE(' ' || s.otchestvo, '') as master_fio,
                        STRING_AGG(u.naimenovanie, ', ') as uslugi,
                        z.obshaya_stoimost,
                        sz.naimenovanie as status_name
                    FROM public.zapis z
                    JOIN public.klient k ON z.id_klienta = k.id_klienta
                    JOIN public.sotrudnik s ON z.id_sotrudnika = s.id_sotrudnika
                    JOIN public.zapis_usluga zu ON z.id_zapisi = zu.id_zapisi
                    JOIN public.usluga u ON zu.id_uslugi = u.id_uslugi
                    JOIN public.status_zapisi sz ON z.id_statusa_zapisi = sz.id_statusa_zapisi
                    GROUP BY z.id_zapisi, z.vremya_data, klient_fio, master_fio, z.obshaya_stoimost, sz.naimenovanie
                    ORDER BY z.vremya_data DESC";

                using var cmd = new NpgsqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                AllZapisi.Clear();
                while (reader.Read())
                {
                    AllZapisi.Add(new ZapisView
                    {
                        IdZapisi = reader.GetInt32(0),
                        VremyaData = reader.GetDateTime(1),
                        KlientFIO = reader.GetString(2),
                        MasterFIO = reader.GetString(3),
                        Usluga = reader.GetString(4),
                        Stoimost = reader.GetInt32(5),
                        Status = reader.GetString(6)
                    });
                }

                Zapisi = new List<ZapisView>(AllZapisi);
                ZapisiDataGrid.ItemsSource = Zapisi;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка загрузки записей:\n" + ex.Message);
            }
        }

        private void Filter_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            string search = SearchBox?.Text.Trim().ToLower() ?? "";
            string? status = (StatusFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString();

            var filtered = AllZapisi.AsEnumerable();

            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(z =>
                    z.KlientFIO.ToLower().Contains(search) ||
                    z.MasterFIO.ToLower().Contains(search) ||
                    z.Usluga.ToLower().Contains(search));
            }

            if (status != null && status != "Все записи")
            {
                filtered = filtered.Where(z => z.Status == status);
            }

            Zapisi = filtered.ToList();
            ZapisiDataGrid.ItemsSource = null;
            ZapisiDataGrid.ItemsSource = Zapisi;
        }

        private void AddZapis_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddZapisWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadZapisi();
                Filter_TextChanged(sender, e);
            }
        }
    }
}