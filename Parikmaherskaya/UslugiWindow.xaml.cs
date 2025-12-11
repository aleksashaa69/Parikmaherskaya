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
    public partial class UslugiWindow : Window
    {
        private readonly string connString =
            "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=22ip22";

        public List<Usluga> AllUslugi { get; set; } = new();
        public List<Usluga> Uslugi { get; set; } = new();

        public UslugiWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadUslugi();
        }

        private void LoadUslugi()
        {
            try
            {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();

                using var cmd = new NpgsqlCommand("SELECT id_uslugi, naimenovanie, opisanie, cena, dlitelnost FROM public.usluga ORDER BY id_uslugi", conn);
                using var reader = cmd.ExecuteReader();

                AllUslugi.Clear();
                while (reader.Read())
                {
                    AllUslugi.Add(new Usluga
                    {
                        IdUslugi = reader.GetInt32(0),
                        Naimenovanie = reader.GetString(1),
                        Opisanie = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Cena = reader.GetInt32(3),
                        Dlitelnost = reader.GetTimeSpan(4)
                    });
                }

                Uslugi = new List<Usluga>(AllUslugi);
                UslugiDataGrid.ItemsSource = Uslugi;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки услуг:\n" + ex.Message);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = SearchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(search))
            {
                Uslugi = new List<Usluga>(AllUslugi);
            }
            else
            {
                Uslugi = AllUslugi.Where(u =>
                    u.Naimenovanie.ToLower().Contains(search) ||
                    u.Opisanie.ToLower().Contains(search)).ToList();
            }
            UslugiDataGrid.ItemsSource = null;
            UslugiDataGrid.ItemsSource = Uslugi;
        }
    }
}