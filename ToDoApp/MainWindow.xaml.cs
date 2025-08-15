using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.ComponentModel;
using ToDoApp.ViewModels;
using ToDoApp.Models;

namespace ToDoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private MainViewModel VM => DataContext as MainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            VM?.Save();
        }

        // In MainWindow.xaml.cs or use an attached behavior
        

    }
}
