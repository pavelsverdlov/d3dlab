using D3DLab.Viewer.Infrastructure;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using WPFLab;
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
            
            Name = PathHelper.GetPathWithMiddleSkipping(fullPath);
        }

        public string FullPath { get; }
        public string Name { get; }
    }
    class OpenFilesViewModel : BaseNotify {
        readonly AppSettings settings;
        readonly IFileLoader loader;
        string fullPathPreview;

        public ICollectionView RecentFiles { get; }
        public ICommand MouseMoveCommand { get; }
        public ICommand OpenWinFileDialogCommand { get; }
        public ICommand ClearRecentFilesHistoryCommand { get; }

        public string FullPathPreview {
            get => fullPathPreview;
            set => Update(ref fullPathPreview, value);
        }

        readonly ObservableCollection<RecentFileItem> recentFiles;

        public OpenFilesViewModel(AppSettings settings, IFileLoader loader) {
            this.settings = settings;
            this.loader = loader;
            
            recentFiles = new ObservableCollection<RecentFileItem>();
            RecentFiles = CollectionViewSource.GetDefaultView(recentFiles);

            RefreshRecentFiles();
            
            RecentFiles.MoveCurrentToPosition(-1);
            RecentFiles.CurrentChanged += RecentFiles_CurrentChanged;

            MouseMoveCommand = new WpfActionCommand<RecentFileItem>(OnMouseMove);
            OpenWinFileDialogCommand = new WpfActionCommand(OnOpenWinFileDialog);
            ClearRecentFilesHistoryCommand = new WpfActionCommand(OnClearRecentFilesHistory);
        }

        void OnClearRecentFilesHistory() {
            recentFiles.Clear();
            settings.ClearReceintFiles();
        }
        void RefreshRecentFiles() {
            recentFiles.Clear();
            foreach (var p in settings.GetReceintFiles()) {
                recentFiles.Add(new RecentFileItem(p));
            }
            RecentFiles.Refresh();
        }
        void OnOpenWinFileDialog() {
            var files = WindowsDefaultDialogs.OpenFolderDialog(
                new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
                WindowsDefaultDialogs.FileFormats.MeshFormats);
            if (files != null) {
                loader.Load(files);
                RefreshRecentFiles();
            }
        }

        void OnMouseMove(RecentFileItem item) {
            FullPathPreview = item.FullPath;
        }

        void RecentFiles_CurrentChanged(object sender, EventArgs e) {
            if(RecentFiles.CurrentItem is RecentFileItem file) {
                loader.Load(new[] { file.FullPath });
            }
            RefreshRecentFiles();
        }
    }
}
