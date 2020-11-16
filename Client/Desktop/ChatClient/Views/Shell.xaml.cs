using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatClient.Views
{
    /// <summary>
    /// Логика взаимодействия для Shell.xaml
    /// </summary>
    public partial class Shell : Window
    {
        public Shell()
        {
            InitializeComponent();
        }
        private void MoveWindowPressed(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
