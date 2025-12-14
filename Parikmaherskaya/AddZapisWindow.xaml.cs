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
                    "SELECT id_sotrudnika, familiya || ' ' || imya || COALESCE(' ' || otchestvo, '') AS fio " +
                    "FROM public.sotrudnik " +
                    "WHERE id_dolzhnosti = 2 " +
                    "ORDER BY familiya", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    MasterComboBox.Items.Add(new { Id = reader.GetInt32(0), FIO = reader.GetString(1) });
                }
                reader.Close();

                using var cmd2 = new NpgsqlCommand("SELECT id_uslugi, naimenovanie FROM public.usluga ORDER BY naimenovanie", conn);
                using var reader2 = cmd2.ExecuteReader();
                while (reader2.Read())
                {
                    UslugaComboBox.Items.Add(new { Id = reader2.GetInt32(0), Naimenovanie = reader2.GetString(1) });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FamiliyaBox.Text) ||
                string.IsNullOrWhiteSpace(ImyaBox.Text) ||
                MasterComboBox.SelectedItem == null ||
                UslugaComboBox.SelectedItem == null ||
                DatePicker.SelectedDate == null ||
                !TimeSpan.TryParse(TimeBox.Text, out TimeSpan time))
            {
                MessageBox.Show("Заполните все обязательные поля правильно!", "Ошибка");
                return;
            }

            try
            {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();
                using var trans = conn.BeginTransaction();

                var clientCmd = new NpgsqlCommand(
                    @"INSERT INTO public.klient (familiya, imya, otchestvo, telefon, el_pochta)
                      VALUES (@fam, @imya, @otch, @tel, @email)
                      RETURNING id_klienta", conn, trans);

                clientCmd.Parameters.AddWithValue("fam", FamiliyaBox.Text.Trim());
                clientCmd.Parameters.AddWithValue("imya", ImyaBox.Text.Trim());
                clientCmd.Parameters.AddWithValue("otch", string.IsNullOrWhiteSpace(OtchestvoBox.Text) ? (object)DBNull.Value : OtchestvoBox.Text.Trim());
                clientCmd.Parameters.AddWithValue("tel", string.IsNullOrWhiteSpace(TelefonBox.Text) ? "+7 (000) 000-00-00" : TelefonBox.Text.Trim());
                clientCmd.Parameters.AddWithValue("email", string.IsNullOrWhiteSpace(EmailBox.Text) ? "no@email.ru" : EmailBox.Text.Trim());

                int klientId = (int)clientCmd.ExecuteScalar();

                dynamic usluga = UslugaComboBox.SelectedItem;
                int cena = GetUslugaCena(usluga.Id, conn);

                DateTime dateTime = DatePicker.SelectedDate.Value.Date + time;

                var zapisCmd = new NpgsqlCommand(
                    @"INSERT INTO public.zapis (vremya_data, obshaya_stoimost, id_klienta, id_sotrudnika, id_statusa_zapisi)
                      VALUES (@dt, @cost, @klient, @master, 1)
                      RETURNING id_zapisi", conn, trans);

                zapisCmd.Parameters.AddWithValue("dt", dateTime);
                zapisCmd.Parameters.AddWithValue("cost", cena);
                zapisCmd.Parameters.AddWithValue("klient", klientId);
                zapisCmd.Parameters.AddWithValue("master", ((dynamic)MasterComboBox.SelectedItem).Id);

                int zapisId = (int)zapisCmd.ExecuteScalar();

                var linkCmd = new NpgsqlCommand(
                    "INSERT INTO public.zapis_usluga (id_zapisi, id_uslugi) VALUES (@z, @u)", conn, trans);
                linkCmd.Parameters.AddWithValue("z", zapisId);
                linkCmd.Parameters.AddWithValue("u", usluga.Id);
                linkCmd.ExecuteNonQuery();

                trans.Commit();

                MessageBox.Show("Запись успешно создана!", "Успех");
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private int GetUslugaCena(int uslugaId, NpgsqlConnection conn)
        {
            var cmd = new NpgsqlCommand("SELECT cena FROM public.usluga WHERE id_uslugi = @id", conn);
            cmd.Parameters.AddWithValue("id", uslugaId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}