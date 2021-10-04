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
using TUSUR_Journal.Windows;
using OpenQA.Selenium;
using System.IO;
using System.Threading;

namespace TUSUR_Journal
{
    /// <summary>
    /// Логика взаимодействия для LinkWindow.xaml
    /// </summary>
    public partial class LinkWindow : Window
    {
        StartWindowData data = new StartWindowData();

        public LinkWindow(StartWindowData data)
        {
           this.data = data;
           InitializeComponent();
        }

        private void StartWork(object sender, RoutedEventArgs e)
        {
            if (!LinkOnLesson.Text.Contains("https://sdo.tusur.ru/mod/bigbluebuttonbn/view.php?id="))
            {
                ShowError("Ссылка указана некорректно");
                return;
            }

            data.link = LinkOnLesson.Text;

            StudentsWindow studentsWindow = new StudentsWindow(data);
            Close();
            studentsWindow.Show();
        }

        private void ShowExampleWindow(object sender, RoutedEventArgs e)
        {
            ExampleWindow window = new ExampleWindow();
            window.ShowDialog();
        }

        private void ShowError(string str)
        {
            ErrorSnackbar.Message.Content = str;
            ShowSnackBar();
        }

        public void ShowSnackBar()
        {
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    ErrorSnackbar.Opacity = 0;
                    ErrorSnackbar.Visibility = Visibility.Visible;
                });

                for (double i = 0; i < 1; i += 0.1)
                {
                    Dispatcher.Invoke(() => ErrorSnackbar.Opacity = i);
                    Thread.Sleep(30);
                }

                Thread.Sleep(3000);

                for (double i = 1; i > 0; i -= 0.1)
                {
                    Dispatcher.Invoke(() => ErrorSnackbar.Opacity = i);
                    Thread.Sleep(30);
                }

                Dispatcher.Invoke(() => ErrorSnackbar.Visibility = Visibility.Hidden);
            });
        }
    }
}
