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
    public partial class SotrudnikiWindow : Window
    {
        private readonly string connString =
            "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=22ip22";

        private List<Sotrudnik> AllSotrudniki { get; set; } = new();
        private List<Sotrudnik> Sotrudniki { get; set; } = new();

        public SotrudnikiWindow()
        {
            InitializeComponent();
            Loaded += SotrudnikiWindow_Loaded;
        }

        private void SotrudnikiWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSotrudniki();
        }

        private void LoadSotrudniki()
        {
            try
            {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();

                string sql = @"
                    SELECT 
                        s.id_sotrudnika,
                        s.familiya, s.imya, s.otchestvo,
                        s.telefon, s.el_pochta,
                        d.naimenovanie as dolzhnost,
                        a.login,
                        STRING_AGG(sp.naimenovanie, ', ') as specializacii
                    FROM public.sotrudnik s
                    JOIN public.dolzhnost d ON s.id_dolzhnosti = d.id_dolzhnosti
                    JOIN public.avtoryzaciya a ON s.id_avtorizatciyi = a.id_avtoryzaciyi
                    LEFT JOIN public.master_specializaciya ms ON s.id_sotrudnika = ms.id_sotrudnika
                    LEFT JOIN public.specializaciya sp ON ms.id_specializaciyi = sp.id_specializaciyi
                    GROUP BY s.id_sotrudnika, d.naimenovanie, a.login
                    ORDER BY s.id_sotrudnika";

                using var cmd = new NpgsqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                AllSotrudniki.Clear();
                while (reader.Read())
                {
                    AllSotrudniki.Add(new Sotrudnik
                    {
                        IdSotrudnika = reader.GetInt32(0),
                        Familiya = reader.GetString(1),
                        Imya = reader.GetString(2),
                        Otchestvo = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Telefon = reader.GetString(4),
                        Email = reader.GetString(5),
                        Dolzhnost = reader.GetString(6),
                        Login = reader.GetString(7),
                        Specializacii = reader.IsDBNull(8) ? "—" : reader.GetString(8)
                    });
                }

                Sotrudniki = new List<Sotrudnik>(AllSotrudniki);
                SotrudnikiDataGrid.ItemsSource = Sotrudniki;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка загрузки сотрудников:\n" + ex.Message);
            }
        }

        private void Filter_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            string search = SearchBox?.Text.Trim().ToLower() ?? "";
            string? dolzhnost = (DolzhnostFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString();

            var filtered = AllSotrudniki.AsEnumerable();

            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(s =>
                    s.FIO.ToLower().Contains(search) ||
                    s.Telefon.Contains(search) ||
                    s.Email.ToLower().Contains(search));
            }

            if (dolzhnost != null && dolzhnost != "Все сотрудники")
            {
                filtered = filtered.Where(s => s.Dolzhnost == dolzhnost);
            }

            Sotrudniki = filtered.ToList();
            SotrudnikiDataGrid.ItemsSource = null;
            SotrudnikiDataGrid.ItemsSource = Sotrudniki;
        }
    }
}