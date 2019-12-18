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
using Microsoft.Win32;

namespace WatchedFile
{
    /// <summary>
    /// Interaktionslogik für ChangeFile.xaml
    /// </summary>
    public partial class ChangeFile : Window
    {
        private event EventHandler TagsChanged;

        public List<ViewModels.Tag> allTags = new List<ViewModels.Tag>();
        public ViewModels.WatchedFile sourceFile = new ViewModels.WatchedFile(); 

        public ChangeFile(List<ViewModels.Tag> AllTags, ViewModels.WatchedFile file)
        {
            InitializeComponent(); 
            allTags = AllTags;            
            if (file == null)
            {
                Title = "Select File";
                B_SelectFile.Content = "Select File";
                file = new ViewModels.WatchedFile("name not set", "no path selected", new System.Collections.ObjectModel.ObservableCollection<ViewModels.Tag>(), new System.Collections.ObjectModel.ObservableCollection<ViewModels.WatchedFile>());
            }
            else
            {
                Title = "Change File";
                B_SelectFile.Content = "Change File";
            }
            sourceFile = file;
            Loaded += ChangeFile_Loaded;
            TagsChanged += ChangeFile_TagsChanged;
            TagsChanged(this, new EventArgs());
            
        }

        private void ChangeFile_TagsChanged(object sender, EventArgs e)
        {
            TB_Tags.Text = string.Empty;
            foreach (ViewModels.Tag tag in sourceFile.Tags)
            {
                TB_Tags.Text += tag.Value + ", ";
            }
        }

        private void ChangeFile_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = sourceFile;
            CB_Tags.ItemsSource = allTags;
            CB_Tags.DisplayMemberPath = "Value";
            CB_RemoveTag.ItemsSource = sourceFile.Tags;
            CB_RemoveTag.DisplayMemberPath = "Value";
        }

        private void B_SelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileOk += Ofd_FileOk;
            ofd.ShowDialog();
        }

        private void Ofd_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TB_Path.Text = ( (OpenFileDialog)sender ).FileName;
        }

        private void CB_Tags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            B_AddTag.IsEnabled = ( CB_Tags.SelectedItem != null ) ? true : false;
        }

        private void TB_NewTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (( (TextBox)sender ).Text != null && ( (TextBox)sender ).Text != string.Empty && ( (TextBox)sender ).Text.Replace(" ", "") != string.Empty)
                B_NewTag.IsEnabled = true;
            else B_NewTag.IsEnabled = false;

            string typedString = TB_NewTag.Text;
            List<string> suggestionList = new List<string>();
            suggestionList.Clear();
            foreach (ViewModels.Tag existingTag in allTags)
            {
                if (!string.IsNullOrEmpty(typedString))
                {
                    if(existingTag.Value.ToLower().StartsWith(typedString.ToLower()) ||
                        existingTag.Value.ToLower().EndsWith(typedString.ToLower())||
                        existingTag.Value.ToLower().Contains(typedString.ToLower()))
                    {
                        suggestionList.Add(existingTag.Value);
                    }
                }
            }

            if (suggestionList.Count > 0)
            {
                LB_NewTagSuggestion.ItemsSource = suggestionList;
                LB_NewTagSuggestion.Visibility = Visibility.Visible;                
            }
            else
            {
                LB_NewTagSuggestion.Visibility = Visibility.Collapsed;
                LB_NewTagSuggestion.ItemsSource = null;
            }
        }

        private void TB_Path_TextChanged(object sender, TextChangedEventArgs e)
        {
            ( (TextBox)sender ).ToolTip = ( (TextBox)sender ).Text;
        }

        private void B_AddTag_Click(object sender, RoutedEventArgs e)
        {            
            if (sourceFile.Tags.Contains( (ViewModels.Tag)CB_Tags.SelectedItem))
            {
                MessageBox.Show("gibts schon");
                return;
            }

            sourceFile.Tags.Add((ViewModels.Tag)CB_Tags.SelectedItem);
            TagsChanged(this, new TagsChangedEventArgs((ViewModels.Tag)CB_Tags.SelectedItem,"+"));            
        }

        private void B_NewTag_Click(object sender, RoutedEventArgs e)
        {
            if (TB_Tags.Text.Contains("'" + TB_NewTag.Text + "',"))
            {
                MessageBox.Show("The Tag '" + TB_NewTag.Text + "' is already assigned to the file.");
                return;
            }

            ViewModels.Tag addTag = new ViewModels.Tag(TB_NewTag.Text);
            sourceFile.Tags.Add(addTag);
            if (!allTags.Contains(addTag))
            {
                allTags.Add(addTag);
            }
            TagsChanged(this, new EventArgs());
        }

        private void B_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void B_Ok_Click(object sender, RoutedEventArgs e)
        {
            sourceFile.Name = TB_Name.Text;
            sourceFile.Path = TB_Path.Text;            
            DialogResult = true;            
            Close();
        }

        private void B_RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            sourceFile.Tags.Remove((ViewModels.Tag)CB_RemoveTag.SelectedItem);
            TagsChanged(this, new EventArgs());
        }

        private void CB_RemoveTag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            B_RemoveTag.IsEnabled = ( CB_RemoveTag.SelectedItem != null ) ? true : false;
        }

        private void LB_NewTagSuggestion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LB_NewTagSuggestion.ItemsSource!=null)
            {
                LB_NewTagSuggestion.Visibility = Visibility.Collapsed;
                TB_NewTag.TextChanged -= TB_NewTag_TextChanged;
                if (LB_NewTagSuggestion.SelectedIndex != -1)
                {
                    TB_NewTag.Text = LB_NewTagSuggestion.SelectedItem.ToString();
                }
                TB_NewTag.TextChanged += TB_NewTag_TextChanged;
            }
        }
    }

    class TagsChangedEventArgs: EventArgs
    {
        ViewModels.Tag _changedTag { get; set; }
        string _done { get; set; }

        public TagsChangedEventArgs() : this(null, null) { }
            

        public TagsChangedEventArgs(ViewModels.Tag Tag, string done)
        {
            if (Tag != null && done != null)
            {
                _changedTag = Tag;
                _done = done;
            }
        }
    }

}
