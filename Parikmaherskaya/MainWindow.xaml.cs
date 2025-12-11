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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=22ip22";

            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    ConnectionStatus.Text = "Подключено успешно! Версия PostgreSQL: " + conn.PostgreSqlVersion;
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus.Text = "Ошибка подключения: " + ex.Message;
            }
        }
    }
}