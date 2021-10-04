using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TUSUR_Journal.Properties;
using OpenQA.Selenium;
using System.Threading;
using System.Windows.Threading;

namespace TUSUR_Journal
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        StartWindowData data = new StartWindowData();
        Thread thread;

        public StartWindow()
        {
            InitializeComponent();
            LoginTB.Text = Settings.Default.LoginBox;
            PasswordTB.Password = Settings.Default.PasswordBox;
            StudentFileTB.Text = Settings.Default.FileBox;
            SaveFolderTB.Text = Settings.Default.FolderBox;
            SaveCheckBox.IsChecked = Settings.Default.CheckBox;
        }

        private void OpenFileDialog(object sender, RoutedEventArgs e)
        {
            using (var openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.ShowDialog();
                string path = openFileDialog.FileName;

                if (!string.IsNullOrWhiteSpace(path))
                {
                    StudentFileTB.Text = path;
                }
            }
        }

        private void OpenFolderDialog(object sender, RoutedEventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.ShowDialog();
                string path = folderBrowserDialog.SelectedPath;

                if (!string.IsNullOrWhiteSpace(path))
                {
                    SaveFolderTB.Text = path;
                }
            }
        }

        private void Program(object sender, RoutedEventArgs e)
        {
            if (SaveCheckBox.IsChecked == true)
            {
                Settings.Default.LoginBox = LoginTB.Text;
                Settings.Default.PasswordBox = PasswordTB.Password;
                Settings.Default.FileBox = StudentFileTB.Text;
                Settings.Default.FolderBox = SaveFolderTB.Text;
                Settings.Default.CheckBox = true;
            }
            else
            {
                Settings.Default.LoginBox = "";
                Settings.Default.PasswordBox = "";
                Settings.Default.FileBox = "";
                Settings.Default.FolderBox = "";
                Settings.Default.CheckBox = false;
            }
            Settings.Default.Save();

            data.login = LoginTB.Text;
            data.password = PasswordTB.Password;
            data.filepath = StudentFileTB.Text;
            data.savepath = SaveFolderTB.Text;

            ParameterizedThreadStart paramThread = new ParameterizedThreadStart(HaveErrors);
            thread = new Thread(paramThread);
            thread.Start(data);

            StartPanel.Visibility = Visibility.Hidden;
            DialogHost.IsOpen = true;
        }

        private void HaveErrors(object pocket)
        {
            Dispatcher.Invoke(() =>
            {
                CheckBox1.IsChecked = false;
                CheckBox2.IsChecked = false;
                CheckBox3.IsChecked = false;
                CheckBox4.IsChecked = false;
            });

            Thread.Sleep(1000);

            StartWindowData data = pocket as StartWindowData;

            bool Empty(string str)
            {
                return string.IsNullOrWhiteSpace(str);
            }

            if (Empty(data.login) || Empty(data.password) || Empty(data.filepath) || Empty(data.savepath))
            {
                ShowError("Заполните все поля!");
                return;
            }
            else
            {
                Dispatcher.Invoke(() => CheckBox1.IsChecked = true);
            }

            IWebDriver Brows = null;

            IWebElement Find(string xPath)
            {
                return Brows.FindElement(By.XPath(xPath));
            }

            bool error = false;
            OpenQA.Selenium.PhantomJS.PhantomJSDriverService driver;

            try
            {
                driver = OpenQA.Selenium.PhantomJS.PhantomJSDriverService.CreateDefaultService();
                driver.HideCommandPromptWindow = true;
            }
            catch
            {
                ShowError("Ошибка входа. Необходимо установить PhantomJS");
                return;
            }


            try
            {
                Brows = new OpenQA.Selenium.PhantomJS.PhantomJSDriver(driver);
                Brows.Navigate().GoToUrl("https://sdo.tusur.ru/login/index.php");
                Brows.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(2));
                
                IWebElement element = Find("//input[@id='username']");
                element.SendKeys(data.login);
                
                element = Find("//input[@id='password']");
                element.SendKeys(data.password + OpenQA.Selenium.Keys.Enter);
                
                element = Find("//input[@id='user_email']");
                element.SendKeys(data.login);

                element = Find("//input[@id='user_password']");
                element.SendKeys(data.password + OpenQA.Selenium.Keys.Enter);
            }
            catch (Exception err)
            {
                Dispatcher.Invoke(() => System.Windows.Forms.MessageBox.Show(err.Message));
                ShowError("Ошибка входа, возомжно, необходимо обновить библиотеки");
                error = true;
            }

            if (error)
            {
                Brows.Quit();
                return;
            }

            Thread.Sleep(2000);

            if (Brows.Title != "ТУСУР")
            {
                ShowError("Неверный логин или пароль");
                Brows.Quit();
                return;
            }
            else
            {
                Brows.Quit();
                Dispatcher.Invoke(() =>
                {
                    CheckBox2.IsChecked = true;
                });
            }

            Thread.Sleep(1000);

            try
            {
                StreamReader stream = new StreamReader(data.filepath);
                stream.Close();

                if (System.IO.Path.GetExtension(data.filepath) != ".txt")
                {
                    ShowError("Файл должен иметь расширение .txt");
                    return;
                }

                Dispatcher.Invoke(() => CheckBox3.IsChecked = true);
            }
            catch
            {
                ShowError("Неверный путь к файлу со студентами");
                return;
            }

            Thread.Sleep(1000);

            if (!Directory.Exists(data.savepath))
            {
                ShowError("Неверный путь к папке сохранения");
                return;
            }

            Dispatcher.Invoke(() => CheckBox4.IsChecked = true);
        }

        private void ShowError(string str)
        {
            Dispatcher.Invoke(() =>
            {
                StartPanel.Visibility = Visibility.Visible;
                DialogHost.IsOpen = false;
                ErrorSnackbar.Message.Content = str;
            });
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

        private void CheckBox4_Checked(object sender, RoutedEventArgs e)
        {
            LinkWindow linkWindow = new LinkWindow(data);
            Close();
            linkWindow.Show();
        }
    }
}
