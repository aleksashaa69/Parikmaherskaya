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
    public partial class AddZapisWindow : Window
    {
        private readonly string connString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=22ip22";

        public AddZapisWindow()
        {
            InitializeComponent();
            LoadMastersAndUslugi();
        }

        private void LoadMastersAndUslugi()
        {
            try
            {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    @"SELECT id_sotrudnika, familiya || ' ' || imya || COALESCE(' ' || otchestvo, '') as fio 
                      FROM public.sotrudnik 
                      WHERE id_dolzhnosti = 2 
                      ORDER BY familiya", conn);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    MasterComboBox.Items.Add(new { Id = r.GetInt32(0), FIO = r.GetString(1) });
                }
                r.Close();

                using var cmd2 = new NpgsqlCommand("SELECT id_uslugi, naimenovanie, cena FROM public.usluga ORDER BY naimenovanie", conn);
                using var r2 = cmd2.ExecuteReader();
                while (r2.Read())
                {
                    UslugaComboBox.Items.Add(new { Id = r2.GetInt32(0), Naimenovanie = r2.GetString(1), Cena = r2.GetInt32(2) });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ClientFIOBox.Text) ||
                MasterComboBox.SelectedItem == null ||
                UslugaComboBox.SelectedItem == null ||
                DatePicker.SelectedDate == null ||
                !TimeSpan.TryParse(TimeTextBox.Text, out TimeSpan time))
            {
                MessageBox.Show("Заполните все поля корректно!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();
                using var transaction = conn.BeginTransaction();

                var clientCmd = new NpgsqlCommand(
                    @"INSERT INTO public.klient (familiya, imya, otchestvo, telefon, el_pochta)
                      VALUES (@fio, '', NULL, '+7 (000) 000-00-00', 'temp@mail.ru')
                      RETURNING id_klienta", conn, transaction);
                clientCmd.Parameters.AddWithValue("fio", ClientFIOBox.Text.Trim());
                int klientId = (int)clientCmd.ExecuteScalar()!;

                DateTime dateTime = DatePicker.SelectedDate.Value.Date + time;
                dynamic usluga = UslugaComboBox.SelectedItem;
                int cena = usluga.Cena;

                var zapisCmd = new NpgsqlCommand(
                    @"INSERT INTO public.zapis 
                      (vremya_data, obshaya_stoimost, id_klienta, id_sotrudnika, id_statusa_zapisi)
                      VALUES (@dt, @cost, @klient, @master, 1)
                      RETURNING id_zapisi", conn, transaction);
                zapisCmd.Parameters.AddWithValue("dt", dateTime);
                zapisCmd.Parameters.AddWithValue("cost", cena);
                zapisCmd.Parameters.AddWithValue("klient", klientId);
                zapisCmd.Parameters.AddWithValue("master", ((dynamic)MasterComboBox.SelectedItem).Id);
                int zapisId = (int)zapisCmd.ExecuteScalar()!;

                var linkCmd = new NpgsqlCommand(
                    "INSERT INTO public.zapis_usluga (id_zapisi, id_uslugi) VALUES (@z, @u)", conn, transaction);
                linkCmd.Parameters.AddWithValue("z", zapisId);
                linkCmd.Parameters.AddWithValue("u", usluga.Id);
                linkCmd.ExecuteNonQuery();

                transaction.Commit();

                MessageBox.Show("Запись успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании записи:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}