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
using System.Windows.Threading;

namespace WatchedFile
{
    /// <summary>
    /// Interaktionslogik für Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
        MainWindow mw = new MainWindow();
        DispatcherTimer t1;
        DispatcherTimer t2;
        bool maximum = false;

        public Splash()
        {
            InitializeComponent();                                  
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            t1 = new DispatcherTimer();
            t1.Interval = new TimeSpan(20000);
            t1.Tick += T1_Tick;
            t1.Start();
            
        }

        private void T1_Tick(object sender, EventArgs e)
        {
            t1.Stop();
            t2 = new DispatcherTimer();
            t2.Interval = new TimeSpan(250000);
            t2.Tick += T2_Tick;
            t2.Start();
            PB.Value = 0;

     
        }

        private void T2_Tick(object sender, EventArgs e)
        {
            if (!maximum)
            {
                PB.Maximum = 50;
                PB.Value++;
            }
                        
            if (PB.Value == PB.Maximum)
            {
                this.Close();
                mw.Show();
            }
        }

    }
}
