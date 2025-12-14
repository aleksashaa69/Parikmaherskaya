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
    public partial class EditZapisWindow : Window
    {
        private readonly string connString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=22ip22";
        private readonly int _zapisId;
        private readonly int _klientId;

        public EditZapisWindow(ZapisView zapis)
        {
            InitializeComponent();
            _zapisId = zapis.IdZapisi;

            _klientId = GetKlientIdByZapisId(_zapisId);

            LoadMastersAndUslugi();
            LoadCurrentZapisData(zapis);
        }

        private int GetKlientIdByZapisId(int zapisId)
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT id_klienta FROM public.zapis WHERE id_zapisi = @id", conn);
            cmd.Parameters.AddWithValue("id", zapisId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void LoadMastersAndUslugi()
        {
            try
            {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    @"SELECT id_sotrudnika, familiya || ' ' || imya || COALESCE(' ' || otchestvo, '') AS fio
                      FROM public.sotrudnik
                      WHERE id_dolzhnosti = 2
                      ORDER BY familiya", conn);
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
                    UslugaComboBox.Items.Add(new { Id = reader2.GetInt32(0), Naimenovanie = reader.GetString(1) });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки списков: " + ex.Message);
            }
        }

        private void LoadCurrentZapisData(ZapisView zapis)
        {
            LoadKlientData(_klientId);

            MasterComboBox.SelectedItem = MasterComboBox.Items.Cast<dynamic>()
                .FirstOrDefault(m => m.FIO == zapis.MasterFIO);

            UslugaComboBox.SelectedItem = UslugaComboBox.Items.Cast<dynamic>()
                .FirstOrDefault(u => zapis.Usluga.Contains(u.Naimenovanie));

            DatePicker.SelectedDate = zapis.VremyaData.Date;
            TimeBox.Text = zapis.VremyaData.ToString("HH:mm");

            StatusComboBox.SelectedItem = StatusComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(i => i.Content.ToString() == zapis.Status);
        }

        private void LoadKlientData(int klientId)
        {
            try
            {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();
                using var cmd = new NpgsqlCommand(
                    "SELECT familiya, imya, otchestvo, telefon, el_pochta FROM public.klient WHERE id_klienta = @id", conn);
                cmd.Parameters.AddWithValue("id", klientId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    FamiliyaBox.Text = reader.GetString(0);
                    ImyaBox.Text = reader.GetString(1);
                    OtchestvoBox.Text = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    TelefonBox.Text = reader.GetString(3);
                    EmailBox.Text = reader.GetString(4);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки клиента: " + ex.Message);
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
                    @"UPDATE public.klient
                      SET familiya = @fam, imya = @imya, otchestvo = @otch, telefon = @tel, el_pochta = @email
                      WHERE id_klienta = @id", conn, trans);

                clientCmd.Parameters.AddWithValue("fam", FamiliyaBox.Text.Trim());
                clientCmd.Parameters.AddWithValue("imya", ImyaBox.Text.Trim());
                clientCmd.Parameters.AddWithValue("otch", string.IsNullOrWhiteSpace(OtchestvoBox.Text) ? (object)DBNull.Value : OtchestvoBox.Text.Trim());
                clientCmd.Parameters.AddWithValue("tel", TelefonBox.Text.Trim());
                clientCmd.Parameters.AddWithValue("email", EmailBox.Text.Trim());
                clientCmd.Parameters.AddWithValue("id", _klientId);
                clientCmd.ExecuteNonQuery();

                DateTime newDateTime = DatePicker.SelectedDate.Value.Date + time;
                dynamic master = MasterComboBox.SelectedItem;
                dynamic usluga = UslugaComboBox.SelectedItem;
                int newCena = GetUslugaCena(usluga.Id, conn);

                var zapisCmd = new NpgsqlCommand(
                    @"UPDATE public.zapis
                      SET vremya_data = @dt, obshaya_stoimost = @cost, id_sotrudnika = @master, id_statusa_zapisi = (SELECT id_statusa_zapisi FROM public.status_zapisi WHERE naimenovanie = @status)
                      WHERE id_zapisi = @id", conn, trans);

                zapisCmd.Parameters.AddWithValue("dt", newDateTime);
                zapisCmd.Parameters.AddWithValue("cost", newCena);
                zapisCmd.Parameters.AddWithValue("master", master.Id);
                zapisCmd.Parameters.AddWithValue("status", (StatusComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "В ожидании");
                zapisCmd.Parameters.AddWithValue("id", _zapisId);
                zapisCmd.ExecuteNonQuery();

                var deleteLink = new NpgsqlCommand("DELETE FROM public.zapis_usluga WHERE id_zapisi = @id", conn, trans);
                deleteLink.Parameters.AddWithValue("id", _zapisId);
                deleteLink.ExecuteNonQuery();

                var insertLink = new NpgsqlCommand("INSERT INTO public.zapis_usluga (id_zapisi, id_uslugi) VALUES (@z, @u)", conn, trans);
                insertLink.Parameters.AddWithValue("z", _zapisId);
                insertLink.Parameters.AddWithValue("u", usluga.Id);
                insertLink.ExecuteNonQuery();

                trans.Commit();

                MessageBox.Show("Запись успешно обновлена!", "Успех");
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении записи:\n" + ex.Message);
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