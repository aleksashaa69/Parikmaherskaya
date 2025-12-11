using Npgsql;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Parikmaherskaya
{
    public partial class MainWindow : Window
    {
        private readonly string connectionString =
            "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=22ip22";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    @"SELECT COUNT(*) 
                      FROM public.avtoryzaciya 
                      WHERE login = @login AND parol = @parol", conn);

                cmd.Parameters.AddWithValue("login", login);
                cmd.Parameters.AddWithValue("parol", password);

                long count = Convert.ToInt64(cmd.ExecuteScalar());

                if (count == 1)
                {
                    MessageBox.Show($"Добро пожаловать, {login}!",
                        "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);

                    var mainApp = new MainAppWindow(login);
                    mainApp.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка входа",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных:\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}