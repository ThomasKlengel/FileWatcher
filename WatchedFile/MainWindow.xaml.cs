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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using WatchedFile.ViewModels;

namespace WatchedFile
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public WatchedFile.ViewModels.MainViewModel mvm = new WatchedFile.ViewModels.MainViewModel();        
        private ViewModels.WatchedFile selectedWF = null;
        bool doDragDrop = false;

        public MainWindow()
        {
            InitializeComponent();
            //mvm = testPopulate();
            mvm = FileHandling.loadFiles();
            DataContext = mvm;

            //List<ViewModels.Tag> allTags = new List<ViewModels.Tag>();
            //for (int i = 1; i < 10; i++)
            //{
            //    allTags.Add(new ViewModels.Tag("Tag"+i.ToString()));
            //}
            //ViewModels.WatchedFile wf = new ViewModels.WatchedFile("testname", "testpath",
            //    new ObservableCollection<ViewModels.Tag>() {new ViewModels.Tag("Tag_File_1"), new ViewModels.Tag("Tag_File_2") },
            //    new ObservableCollection<ViewModels.WatchedFile>());
      
        }

        private ViewModels.MainViewModel testPopulate()
        {
            ViewModels.MainViewModel mvm2 = new ViewModels.MainViewModel();
            for (int i = 1; i < 7; i++)
            {
                ViewModels.WatchedFile wf_level1 = new ViewModels.WatchedFile("lev1"+i.ToString(),"",new ObservableCollection<ViewModels.Tag>(), new ObservableCollection<ViewModels.WatchedFile>());
                if (i % 2 == 0)
                {
                    ViewModels.WatchedFile wf_level2 = new ViewModels.WatchedFile("lev2"+i.ToString(), "", new ObservableCollection<ViewModels.Tag>(), new ObservableCollection<ViewModels.WatchedFile>());
                    if (i % 4 == 0)
                    {
                        for (int j = 1; j < 3; j++)
                        {
                            ViewModels.WatchedFile wf_level3 = new ViewModels.WatchedFile("lev3_"+i.ToString()+"_"+j.ToString(), "lev3_"+j.ToString(), new ObservableCollection<ViewModels.Tag>(), new ObservableCollection<ViewModels.WatchedFile>());
                            wf_level2.Subs.Add(wf_level3);
                        }
                    }
                    wf_level1.Subs.Add(wf_level2);
                }
                mvm2.WatchedFiles.Add(wf_level1);
            }
            return mvm2;
        }

        void TV_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {                
                ViewModels.WatchedFile draggedItem = ( ( e.Source as TreeView ) ).SelectedItem as ViewModels.WatchedFile;

                if (draggedItem != null)
                {
                    DragDrop.DoDragDrop(TV, draggedItem, DragDropEffects.Move);
                    doDragDrop = true;
                    e.Handled = true;
                }
            }            
        }
        void TV_Drop(object sender, DragEventArgs e)
        {
            if (!doDragDrop)
               return;
            //DependencyObject uie = TV_Sort.InputHitTest(e.GetPosition(TV_Sort)) as DependencyObject;
            ViewModels.WatchedFile dragItem = e.Data.GetData(typeof(ViewModels.WatchedFile)) as ViewModels.WatchedFile;
            if (dragItem == null)
                return;
            ViewModels.WatchedFile dropOnItem = null;
            ObservableCollection<ViewModels.WatchedFile> dropOnList = null;
            switch (( (DependencyObject)e.OriginalSource ).DependencyObjectType.SystemType.Name)
            {
                case "TextBlock":
                    dropOnItem = ( e.OriginalSource as TextBlock ).DataContext as ViewModels.WatchedFile;
                    break;
                case "Image":
                    dropOnItem = ( e.OriginalSource as Image ).DataContext as ViewModels.WatchedFile;
                    break;
                case "Border":
                    dropOnItem = ( e.OriginalSource as Border ).DataContext as ViewModels.WatchedFile;
                    break;
                default:
                    if (e.OriginalSource is Grid)
                        dropOnList = (e.OriginalSource as Grid).DataContext as ObservableCollection<ViewModels.WatchedFile>;
                    else return;
                    break;
            }

            if (dropOnItem != null)
            {
                if (dropOnItem == dragItem)
                    return;
                if (!string.IsNullOrEmpty(dropOnItem.Path))
                {
                    MessageBox.Show("You cant drop something on another file, drop it on a folder instead.");
                    return;
                }
                FileHandling.removeFileFromWatchList(mvm.WatchedFiles, dragItem);
                doDragDrop = true;
                dropOnItem.Subs.Add(dragItem);
                foreach (ViewModels.WatchedFile file in mvm.WatchedFiles)
                {
                    FileHandling.refeshFilesInView(file);
                }
                e.Handled = true;
            }
            else //if (dropOnList == null)
            {
                FileHandling.removeFileFromWatchList(mvm.WatchedFiles, dragItem);
                doDragDrop = true;
                mvm.WatchedFiles.Add(dragItem);
                e.Handled = true;
            }
            doDragDrop = false;
        }

        
 
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            FileHandling.saveFiles(mvm);
        }

        private void StackPanel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            TV.AllowDrop = false;
            doDragDrop = false;
            selectedWF = (ViewModels.WatchedFile)(((StackPanel)sender).DataContext);
            ContextMenu cm_Stack = new ContextMenu();
            
            MenuItem mi_RemoveItem = new MenuItem();
            mi_RemoveItem.Header = "RemoveItem";
            mi_RemoveItem.Click += Mi_RemoveItem_Click;           
            cm_Stack.Items.Add(mi_RemoveItem);

            if (string.IsNullOrEmpty(selectedWF.Path))
            {
                MenuItem mi_AddFolder = new MenuItem();
                mi_AddFolder.Header = "Add folder";
                mi_AddFolder.Click += Mi_AddFolder_Click;
                MenuItem mi_ChangeFolder = new MenuItem();
                mi_AddFolder.Header = "Change folder";
                mi_AddFolder.Click += Mi_ChangeFolder_Click;
                MenuItem mi_AddFile = new MenuItem();
                mi_AddFile.Header = "Add file";
                mi_AddFile.Click += Mi_AddFile_Click;
                cm_Stack.Items.Add(mi_AddFolder);
                cm_Stack.Items.Add(mi_AddFile);
            }
            if (!string.IsNullOrEmpty(selectedWF.Path))
            {
                MenuItem mi_ChangeFile = new MenuItem();
                mi_ChangeFile.Header = "Change file";
                mi_ChangeFile.Click += Mi_ChangeFile_Click;
                cm_Stack.Items.Add(mi_ChangeFile);
            }

            cm_Stack.IsOpen = true;            
        }

        #region ContextMenu
        private void Mi_ChangeFolder_Click(object sender, RoutedEventArgs e)
        {
            ChangeFolder cf = new ChangeFolder(selectedWF);
            if (cf.ShowDialog() == true)
            {
                changeItem(cf.sourceFile);
                //addItem(cf.sourceFile);
            }
        }

        private void Mi_ChangeFile_Click(object sender, RoutedEventArgs e)
        {
            ChangeFile cf = new ChangeFile(FileHandling.allTags,selectedWF);
            if (cf.ShowDialog() == true)
            {
                changeItem(cf.sourceFile);
                //addItem(cf.sourceFile);
            }
        }

        private void Mi_AddFile_Click(object sender, RoutedEventArgs e)
        {
            ChangeFile cf = new ChangeFile(FileHandling.allTags, new ViewModels.WatchedFile());
            if (cf.ShowDialog() == true)
            {
                addItem(cf.sourceFile);
                foreach (Tag t in cf.sourceFile.Tags)
                {
                    if (!FileHandling.allTags.Contains(t))
                        FileHandling.allTags.Add(t);
                }
            }  
                       
        }

        private void Mi_AddFolder_Click(object sender, RoutedEventArgs e)
        {
            ChangeFolder cf = new ChangeFolder(new ViewModels.WatchedFile());
            if (cf.ShowDialog() == true)
            {
                addItem(cf.sourceFile);
            }
        }

        private void Mi_RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedWF.Path)|| string.IsNullOrWhiteSpace(selectedWF.Path))
            {
                if (MessageBox.Show("The chosen object is a folder. Deletion will remove all logical sub folders and files.", "Delete folder and sub items",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) != MessageBoxResult.OK)
                {
                    return;
                }                
            }
            else
            {
                if (MessageBox.Show("Do you really want to remove the item?", "Delete item",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) != MessageBoxResult.OK)
                {
                    return;
                }
            }

            removeFolder(mvm.WatchedFiles, selectedWF);
            selectedWF = null;
            
        }
        #endregion

        private void removeFolder(ObservableCollection<WatchedFile.ViewModels.WatchedFile> folderCollection, WatchedFile.ViewModels.WatchedFile folderToRemove)
        {
            folderCollection.Remove(folderToRemove);
            foreach (WatchedFile.ViewModels.WatchedFile folder in folderCollection)
            {
                 removeFolder(folder.Subs, folderToRemove);
            }
            doDragDrop = true;
        }
        private void addItem(ViewModels.WatchedFile item)
        {
            if (selectedWF == null)            
                mvm.WatchedFiles.Add(item);            
            else
                selectedWF.Subs.Add(item);

            selectedWF = null;
            TV.AllowDrop = true;
        }
        private void changeItem(ViewModels.WatchedFile item)
        {
            if (string.IsNullOrEmpty(selectedWF.Path) || string.IsNullOrWhiteSpace(selectedWF.Path))
                selectedWF.Name = item.Name;
            else
            {
                selectedWF.Path = item.Path;
                selectedWF.Name = item.Name;
                selectedWF.Tags.Clear();
                foreach (ViewModels.Tag tag in item.Tags)
                {
                    selectedWF.Tags.Add(tag);
                }
            }

            selectedWF = null;
            TV.AllowDrop = true;
        }      

        private void TV_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TV.SelectedValuePath = null;
        }
        private void TV_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (TV.SelectedItem!=null)
            {
                ViewModels.WatchedFile selected = ((ViewModels.WatchedFile)TV.SelectedItem);
                B_Explorer.Visibility = Visibility.Collapsed;
                B_Open.Visibility = Visibility.Collapsed;
                G_Show.Children.Clear();
                if (!string.IsNullOrEmpty(selected.Path))
                {
                    if (!File.Exists(selected.Path))
                    {
                        if (MessageBox.Show("Die Datei wurde gelöscht oder verschoben. Wollen sie die Datei aus der Liste entfernen?",
                            "Datei nicht gefunden",MessageBoxButton.YesNo,MessageBoxImage.Exclamation)== MessageBoxResult.Yes)
                        {
                            FileHandling.removeFileFromWatchList(mvm.WatchedFiles,selected);
                            doDragDrop = true;
                        }
                        return;
                    }              
                    if (selected.Path.EndsWith("txt")||selected.Path.EndsWith("xml")||selected.Path.EndsWith("xsd"))
                    {
                        B_Explorer.Visibility = Visibility.Visible;
                        B_Open.Visibility = Visibility.Visible;
                        Rectangle r = new Rectangle();
                        r.Margin = new Thickness(0);
                        r.Stroke = new SolidColorBrush(Colors.Gray);
                        r.StrokeThickness = 1;

                        ScrollViewer sv = new ScrollViewer();                                               
                        sv.Margin = new Thickness(1);
                        sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                        sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                        TextBlock TB = new TextBlock();
                        TB.Margin = new Thickness(0);
                        TB.Text = File.ReadAllText(selected.Path);                      

                        sv.Content = TB;
                        G_Show.Children.Add(sv);
                        G_Show.Children.Add(r);
                    }

                    else if (selected.Path.EndsWith("pdf"))
                    {
                        B_Explorer.Visibility = Visibility.Visible;
                        B_Open.Visibility = Visibility.Visible;
                        Rectangle r = new Rectangle();
                        r.Margin = new Thickness(0);
                        r.Stroke = new SolidColorBrush(Colors.Gray);
                        r.StrokeThickness = 1;

                        ScrollViewer sv = new ScrollViewer();
                        sv.Margin = new Thickness(1);
                        sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                        sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                        WebBrowser WB = new WebBrowser();
                        WB.Margin = new Thickness(0);
                        WB.Navigate("file:///" + selected.Path);

                        sv.Content = WB;
                        G_Show.Children.Add(sv);
                        G_Show.Children.Add(r);
                    }

                }
            }
        }

        private void B_Explorer_Click(object sender, RoutedEventArgs e)
        {
            string path = ((ViewModels.WatchedFile)TV.SelectedItem).Path;            
            System.Diagnostics.Process.Start("explorer.exe", "/e,/select,\"" + path + "\"");
        }
        private void B_Open_Click(object sender, RoutedEventArgs e)
        {         
            System.Diagnostics.Process.Start(((ViewModels.WatchedFile)TV.SelectedItem).Path);
        }
    }
}


namespace WatchedFile.ViewModels
{
    [Serializable()]
    public class ViewModelBase : INotifyPropertyChanged
    {
        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Serializable()]
    public class WatchedFile : ViewModelBase
    {
        #region Name Property
        private String _name = default(String);
        public String Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion Name Property

        #region Path Property
        private String _path = default(String);
        public String Path
        {
            get { return _path; }
            set
            {
                setDisplayImage(value);
                if (value != _path)
                {
                    _path = value;
                    OnPropertyChanged("Path");
                    
                }
            }
        }
        #endregion Path Property

        #region Tags Property
        private ObservableCollection<Tag> _tags = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> Tags
        {
            get { return _tags; }
            protected set
            {
                if (value != _tags)
                {
                    _tags = value;
                    OnPropertyChanged("Tags");
                }
            }
        }
        #endregion Tags Property

        #region Subs Property
        private ObservableCollection<WatchedFile> _subs = new ObservableCollection<WatchedFile>();
        public ObservableCollection<WatchedFile> Subs
        {
            get { return _subs; }
            protected set
            {
                setDisplayImage(Path);
                if (value != _subs)
                {
                    _subs = value;
                    _subs.CollectionChanged += _subs_CollectionChanged;
                    OnPropertyChanged("Subs");
                }
            }
        }

        private void _subs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Subs = Sort(Subs);
            setDisplayImage(Path);
        }
        #endregion Subs Property

        [NonSerialized()]
        private BitmapImage _displayImage = default(BitmapImage);
        public BitmapImage DisplayImage
        {
            get { return _displayImage; }
            protected set
            {
                if (value != _displayImage)
                {
                    _displayImage = value;
                    OnPropertyChanged("DisplayImage");
                }
            }
        }

        private void setDisplayImage(string path)
        {
            System.Drawing.Bitmap dImg;
            if (string.IsNullOrEmpty(path))
            {
                if (_subs.Count > 0)
                    dImg = ( (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject("FolderFull") );                
                else dImg = ( (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject("FolderEmpty") );
            }
            else dImg = ( (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject("Picture") );
            MemoryStream ms = new MemoryStream();
            dImg.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            BitmapImage bImg = new BitmapImage();
            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(ms.ToArray());
            bImg.EndInit();
            DisplayImage = bImg;          

        }

        public WatchedFile(): this(string.Empty,string.Empty,new ObservableCollection<Tag>(),new ObservableCollection<WatchedFile>())
        {                                    
        }

        public WatchedFile(String name, String path, ObservableCollection<Tag> tags, ObservableCollection<WatchedFile> subitems)
        {
            Subs = WatchedFile.Sort(subitems);
            Name = name;
            Path = path;
            Tags = tags;
        }

        public static ObservableCollection<WatchedFile> Sort(ObservableCollection<WatchedFile> files)
        {
            if (files == null)
                return files;
            ObservableCollection<WatchedFile> filesReturn = new ObservableCollection<ViewModels.WatchedFile>();
            WatchedFile[] sortedArray = files.ToArray();

            WatchedFile temp;
            for (int j = 1; j <= sortedArray.Length - 1; j++)
            {
                for (int i = j; i > 0; i--)
                {
                    if (sortedArray[i].Subs != null && sortedArray[i].Subs.Count > 1)
                    {
                        ObservableCollection<WatchedFile> subs = Sort(sortedArray[i].Subs);
                        sortedArray[i].Subs.Clear();
                        foreach (WatchedFile f in subs)
                            sortedArray[i].Subs.Add(f);
                    }

                    if (sortedArray[i - 1].Subs != null && sortedArray[i - 1].Subs.Count > 1)
                    {
                        ObservableCollection<WatchedFile> subs = Sort(sortedArray[i - 1].Subs);
                        sortedArray[i - 1].Subs.Clear();
                        foreach (WatchedFile f in subs)
                            sortedArray[i - 1].Subs.Add(f);
                    }

                    if (( sortedArray[i].Name ).CompareTo(sortedArray[i - 1].Name) == -1)
                    {
                        temp = sortedArray[i];
                        sortedArray[i] = sortedArray[i - 1];
                        sortedArray[i - 1] = temp;
                    }
                    else
                        break;
                }
            }
            filesReturn.Clear();
            foreach (WatchedFile f in sortedArray)
                filesReturn.Add(f);

            return filesReturn;
        }
    }

    [Serializable()]
    public class Tag
    {
        public Tag(String value)
        {
            Value = value;
        }
        public String Value { get; private set; }
    }

    [Serializable()]
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
                           
        }

        #region WatchedFiles Property      
        private ObservableCollection<WatchedFile> _watchedFiles = new ObservableCollection<WatchedFile>();
        public ObservableCollection<WatchedFile> WatchedFiles
        {
            get { return _watchedFiles; }
            protected set
            {
                if (value != _watchedFiles)
                {
                    _watchedFiles =WatchedFile.Sort(value);// value;
                    _watchedFiles.CollectionChanged += _watchedFiles_CollectionChanged;
                    OnPropertyChanged();
                }
            }
        }

        private void _watchedFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            WatchedFiles = WatchedFile.Sort(WatchedFiles);
        }
        #endregion WatchedFiles Property
    }
}
