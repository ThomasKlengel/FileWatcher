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

namespace WatchedFile
{
    /// <summary>
    /// Interaktionslogik für ChangeFolder.xaml
    /// </summary>
    public partial class ChangeFolder : Window
    {
        public ViewModels.WatchedFile sourceFile = new ViewModels.WatchedFile();

        public ChangeFolder(ViewModels.WatchedFile file)
        {
            InitializeComponent();
            if (file == null)
            {
                Title = "Choose Folder";
                file = new ViewModels.WatchedFile("name not set", string.Empty, new System.Collections.ObjectModel.ObservableCollection<ViewModels.Tag>(), new System.Collections.ObjectModel.ObservableCollection<ViewModels.WatchedFile>());
            }
            else
            {
                Title = "Change Folder";
            }
            sourceFile = file;
            Loaded += ChangeFolder_Loaded;
        }

        private void ChangeFolder_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = sourceFile;
        }

        private void B_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void B_Ok_Click(object sender, RoutedEventArgs e)
        {
            sourceFile.Name = TB_Name.Text;
            DialogResult = true;
            Close();
        }
    }
}
