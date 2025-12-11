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
    public partial class MainAppWindow : Window
    {
        public MainAppWindow()
        {
            InitializeComponent();
            UserNameTextBlock.Text = "Пользователь: Неизвестно";
        }

        public MainAppWindow(string login)
        {
            InitializeComponent();
            UserNameTextBlock.Text = $"Пользователь: {login}";
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }

        private void ShowUslugi_Click(object sender, RoutedEventArgs e)
        {
            new UslugiWindow().ShowDialog();
        }

        private void ShowZapisi_Click(object sender, RoutedEventArgs e)
        {
            new ZapisiWindow().ShowDialog();
        }

        private void ShowSotrudniki_Click(object sender, RoutedEventArgs e)
        {
            new SotrudnikiWindow().ShowDialog();
        }

        private void ShowOplata_Click(object sender, RoutedEventArgs e)
        {
            new OplataWindow().ShowDialog();
        }
    }
}