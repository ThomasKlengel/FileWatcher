using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace WatchedFile
{
    class FileHandling
    {
        internal static List<ViewModels.Tag> allTags= new List<ViewModels.Tag>();

        #region Dateien laden/speichern
        internal static ViewModels.MainViewModel loadFiles()
        {
            allTags.Clear();
            BinaryFormatter formatter = new BinaryFormatter();
            ViewModels.MainViewModel mvm1 = new ViewModels.MainViewModel();
            if (File.Exists(@"testbin.xml"))
            {
                FileStream fs_open = new FileStream(@"testbin.xml", FileMode.Open);
                foreach (ViewModels.WatchedFile file in ((ViewModels.MainViewModel)formatter.Deserialize(fs_open)).WatchedFiles)
                {
                    ViewModels.WatchedFile wf = new ViewModels.WatchedFile(file.Name, file.Path, file.Tags, new ObservableCollection<ViewModels.WatchedFile>());
                    foreach (ViewModels.WatchedFile subs in file.Subs)
                    {
                        wf.Subs.Add(refeshFilesInView(subs));
                    }
                    mvm1.WatchedFiles.Add(wf);
                }
                fs_open.Close();
            }
            return mvm1;
        }
        internal static void saveFiles(ViewModels.MainViewModel mvm)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(@"testbin.xml", FileMode.Create);
            formatter.Serialize(stream, mvm);
            stream.Close();
        }
        #endregion
        
        #region FileHandling im ViewModel
        internal static ViewModels.WatchedFile refeshFilesInView(ViewModels.WatchedFile fileSource)
        {
            ViewModels.WatchedFile wf = new ViewModels.WatchedFile(fileSource.Name, fileSource.Path, fileSource.Tags, new ObservableCollection<ViewModels.WatchedFile>());
            foreach (ViewModels.Tag t in wf.Tags)
            {
                if (!allTags.Contains(t))
                    allTags.Add(t);
            }
            foreach (ViewModels.WatchedFile subs in fileSource.Subs)
            {
                wf.Subs.Add(refeshFilesInView(subs));
            }
            return wf;
        }
        internal static void removeFileFromWatchList(ObservableCollection<WatchedFile.ViewModels.WatchedFile> fileCollection, WatchedFile.ViewModels.WatchedFile fileToRemove)
        {
            fileCollection.Remove(fileToRemove);
            foreach (WatchedFile.ViewModels.WatchedFile file in fileCollection)
            {
                if (file.Subs.Count > 0)
                    removeFileFromWatchList(file.Subs, fileToRemove);
            }
        }
        #endregion
    }
}
