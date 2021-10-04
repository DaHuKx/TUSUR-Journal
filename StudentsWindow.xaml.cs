using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;

namespace TUSUR_Journal
{
    /// <summary>
    /// Логика взаимодействия для StudentsWindow.xaml
    /// </summary>
    public partial class StudentsWindow : Window
    {
        List<Student> students;
        IWebDriver Brows;
        int Time = 0;
        int CheckTime = 900;
        int times = 0;
        Thread checkOnline;
        bool stopThread = false;
        string savepath;

        public StudentsWindow(StartWindowData data)
        {
            InitializeComponent();

            savepath = data.savepath;
            
            string[] name;

            using (FileStream fileStream = File.OpenRead(data.filepath))
            {
                byte[] array = new byte[fileStream.Length];
                fileStream.Read(array, 0, array.Length);
                string stud = Encoding.Default.GetString(array);

                name = stud.Split('\n');
            }

            students = new List<Student>();
            for (int i = 0; i < name.Count(); i++)
            {
                Student stud = new Student(name[i].Trim());
                students.Add(stud);
            }

            foreach (Student student in students)
            {
                Border border = new Border();
                border.Visibility = Visibility.Visible;
                border.Background = new SolidColorBrush(Color.FromRgb(201, 201, 201));
                border.Height = 80;
                border.Margin = new Thickness(5);
                border.CornerRadius = new CornerRadius(10);
                border.Child = student.Panel;
                StudentsPanel.Children.Add(border);
            }

            ParameterizedThreadStart parameterizedThread = new ParameterizedThreadStart(BrowserWork);
            Thread work = new Thread(parameterizedThread);
            work.Start(data);
        }

        private void BrowserWork(object data)
        {
            IWebElement Find(string str)
            {
                return Brows.FindElement(By.XPath(str));
            }

            StartWindowData Data = data as StartWindowData; 

            try
            {
                ChromeDriverService driver = ChromeDriverService.CreateDefaultService();
                driver.HideCommandPromptWindow = true;

                Brows = new ChromeDriver(driver, new ChromeOptions());
                Brows.Manage().Window.Maximize();

                Brows.Navigate().GoToUrl(Data.link);
                IWebElement element = Find("//div[@class='logtusur']");
                element.Click();
                element = Find("//input[@id='user_email']");
                element.SendKeys(Data.login);
                element = Find("//input[@id='user_password']");
                element.SendKeys(Data.password + Keys.Enter);
                Brows.Navigate().GoToUrl(Data.link);
                element = Find("//input[@id='join_button_input']");

                List<string> before = Brows.WindowHandles.ToList();
                element.Click();
                Thread.Sleep(2000);
                if (Brows.WindowHandles.Count == 1)
                {
                    Dispatcher.Invoke(() => StageLabel.Content = "Ожидание подключения преподавателя...");
                    while (Brows.WindowHandles.Count == 1)
                    {
                        element.Click();
                        Thread.Sleep(2000);
                    }
                }

                List<string> after = Brows.WindowHandles.ToList();
                Brows.SwitchTo().Window(after.Except(before).ToList()[0]);

                Thread.Sleep(10000);
                element = Find("//i[@class='icon--2q1XXw icon-bbb-listen']");
                element.Click();

                Dispatcher.Invoke(() => InfoPanel.Visibility = Visibility.Visible);
            }
            catch
            {
                Dispatcher.Invoke(() => MessageBox.Show("Возникла ошибка, перезапустите программу.", "Ошибка!", MessageBoxButton.OK));
                Dispatcher.Invoke(() => Close());
            }
        }

        private void InfoPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            checkOnline = new Thread(CheckOnline);
            checkOnline.Start();

            StageLabel.Content = "Проверка " + (times + 1).ToString() + "/5";
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void Tick(object sender, EventArgs e)
        {
            Time++;
            Dispatcher.Invoke(() =>
            {
                if (Time > 60)
                    WorkTimeLabel.Content = (Time / 60).ToString() + " мин " + (Time % 60).ToString() + " сек";
                else
                    WorkTimeLabel.Content = Time.ToString() + " сек";
            });

            if (times < 4)
            {
                CheckTime--;
                Dispatcher.Invoke(() =>
                {
                    if (CheckTime > 60)
                        CheckTimerLabel.Content = (CheckTime / 60).ToString() + " мин " + (CheckTime % 60).ToString() + " сек";
                    else
                        CheckTimerLabel.Content = CheckTime.ToString() + " сек";

                    if (CheckTime == 0)
                    {
                        CheckWithSave(sender, new RoutedEventArgs());
                    }
                });
            }
        }

        private void CheckWithSave(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => CheckStudentsButton.IsEnabled = false);
            foreach (var student in students)
            {
                PackIcon icon = MakeIcon();

                try
                {
                    Brows.FindElement(By.XPath("//span[text()='" + student.Name + "']"));
                    student.IsHere++;
                    icon.Kind = PackIconKind.CheckBold;
                    icon.Foreground = new SolidColorBrush(Color.FromRgb(31, 228, 0));
                    icon.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                }
                catch
                {
                    icon.Kind = PackIconKind.CloseThick;
                    icon.Foreground = Brushes.Red;
                    icon.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                }

                Dispatcher.Invoke(() => student.WorkPanel.Children.Add(icon));
            }

            if (times == 4)
            {
                string time = DateTime.Now.ToString();
                Directory.CreateDirectory(savepath + "/" + time.Split(' ')[0]);
                StreamWriter file = new StreamWriter(savepath + "/" + time.Split(' ')[0] + "/" + time.Split(' ')[1].Replace(':','-') + ".txt");
                
                foreach(var student in students)
                {
                    if (student.IsHere > 2)
                        file.WriteLine(student.Name + " - Был");
                    else
                        file.WriteLine(student.Name + " - не был");
                }

                file.Close();
                StageLabel.Content = "Файл сохранён!";
                CheckTimerLabel.Content = "Завершено.";
                CheckStudentsButton.Visibility = Visibility.Hidden;
                ProgressBar4ik.Visibility = Visibility.Hidden;
                return;
            }

            times++;
            CheckTime = 900;
            Dispatcher.Invoke(() => CheckStudentsButton.IsEnabled = true);
            Dispatcher.Invoke(() => StageLabel.Content = "Проверка " + (times + 1).ToString() + "/5");
        }

        private PackIcon MakeIcon()
        {
            PackIcon icon = new PackIcon();
            icon.Height = 23;
            icon.Width = 23;
            if (times == 0)
                icon.Margin = new Thickness(10, 0, 5, 0);
            else
                icon.Margin = new Thickness(5, 0, 5, 0);
            icon.BorderBrush = Brushes.Black;
            icon.BorderThickness = new Thickness(2);
            return icon;
        }

        private void ShowNotHere(object sender, RoutedEventArgs e)
        {
            string NotHereList = "";
            int studentscount = 0;

            foreach (var student in students)
            {
                try
                {
                    Brows.FindElement(By.XPath("//span[text()='" + student.Name + "']"));
                    studentscount++;
                }
                catch
                {
                    NotHereList += student.Name + "\n";
                }
            }

            HereCountLabel.Content = studentscount.ToString() + "/" + students.Count;

            Task.Run(() =>
            {
                Dispatcher.Invoke(() => MessageBox.Show(NotHereList, "Отсутствующие", MessageBoxButton.OK));
            });
        }

        private void CheckOnline()
        {
            while (!stopThread)
            {
                int count = 0;

                foreach (var student in students)
                {
                    if (stopThread)
                    {
                        break;
                    }

                    try
                    {
                        Brows.FindElement(By.XPath("//span[text()='" + student.Name + "']"));
                        Dispatcher.Invoke(() => student.icon.Foreground = Brushes.Green);
                        count++;
                    }
                    catch
                    {
                        Dispatcher.Invoke(() => student.icon.Foreground = Brushes.Red);
                    }
                }

                if (!stopThread)
                    Dispatcher.Invoke(() => HereCountLabel.Content = count.ToString() + "/" + students.Count);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                stopThread = true;
                checkOnline.Join(2000);
            }
            catch
            {
                //Nothing
            }
        }
    }
}
