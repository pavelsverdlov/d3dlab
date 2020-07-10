using D3DLab.Viewer.Infrastructure;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Data;

using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation.OpenFiles {
    class OpenFilesDialog {

    }
    interface IFileLoader {
        void Load(string[] files);
    }
    class RecentFileItem {
        public RecentFileItem(string fullPath) {
            FullPath = fullPath;

            var root = Path.GetPathRoot(fullPath);
            var name = Path.GetFileName(fullPath);

            Name = $"{root}...\\{name}";
        }

        public string FullPath { get; }
        public string Name { get; }
    }
    class OpenFilesViewModel : BaseNotify {
        readonly AppSettings settings;
        readonly IFileLoader loader;

        public ICollectionView RecentFiles { get; }
        readonly ObservableCollection<RecentFileItem> recentFiles;
        public OpenFilesViewModel(AppSettings settings, IFileLoader loader) {
            this.settings = settings;
            this.loader = loader;
            recentFiles = new ObservableCollection<RecentFileItem>();

            foreach(var p in settings.GetReceintFiles()) {
                recentFiles.Add(new RecentFileItem(p));
            }

            RecentFiles = CollectionViewSource.GetDefaultView(recentFiles);
            RecentFiles.MoveCurrentToPosition(-1);
            RecentFiles.CurrentChanged += RecentFiles_CurrentChanged;
        }

        void RecentFiles_CurrentChanged(object sender, EventArgs e) {
            if(RecentFiles.CurrentItem is RecentFileItem file) {
                loader.Load(new[] { file.FullPath });
            }
        }
    }
}
