using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace TUSUR_Journal
{
    class Student
    {
        private static int idCount = 1;
        public int ID;
        public string Name;
        public int IsHere;
        public StackPanel Panel;
        public StackPanel WorkPanel;
        public PackIcon icon;

        public Student(string name)
        {
            IsHere = 0;
            Panel = new StackPanel();
            WorkPanel = new StackPanel();
            ID = idCount;
            idCount++;
            Name = name;

            StackPanel namepanel = new StackPanel();
            namepanel.Orientation = Orientation.Horizontal;
            namepanel.Margin = new Thickness(5);

            Label FIO = new Label();
            FIO.Content = ID.ToString() + ". " + Name;
            FIO.FontSize = 15;
            FIO.FontWeight = FontWeights.Bold;

            icon = new PackIcon();
            icon.Kind = PackIconKind.Account;
            icon.Width = 25;
            icon.Height = 25;

            WorkPanel.Orientation = Orientation.Horizontal;

            namepanel.Children.Add(icon);
            namepanel.Children.Add(FIO);
            Panel.Children.Add(namepanel);
            Panel.Children.Add(WorkPanel);
        }
    }
}
