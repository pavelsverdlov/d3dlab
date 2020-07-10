using D3DLab.Viewer.Presentation.FileDetails;
using D3DLab.Viewer.Presentation.OpenFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using WPFLab;
using WPFLab.MVVM;
using WPFLab.Threading;

namespace D3DLab.Viewer.Presentation {
    class ProxyDialog<TWin, TVM> where TWin : Window where TVM : BaseNotify {
        readonly NullVerificationLock<TWin> loker;
        readonly Func<TWin> create;
        TWin win;
        public ProxyDialog(Func<TWin> create) {
            loker = new NullVerificationLock<TWin>();
            this.create = create;
        }
        TWin Create() {
            var win = create();
            win.Owner = Application.Current.MainWindow;
            win.Closed += Win_Closed;
            return win;
        }
        void Win_Closed(object sender, EventArgs e) {
            loker.Destroy(ref win, Cleanup);
        }

        TWin Cleanup() {
            win.Closed -= Win_Closed;
            return null;
        }

        public void Open() {
            Open(_=> { });
        }
        public void Open(Action<TVM> init) {
            loker.Create(ref win, Create);
            init((TVM)win.DataContext);
            win.Show();
        }
        public void Close() {
            win.Close();
        }

    }
    class DialogManager {
        public DialogManager(IDependencyResolverService service) {
            ObjDetails = new ProxyDialog<ObjDetailsWindow, ObjDetailsViewModel>(
                ()=> service.ResolveView<ObjDetailsWindow, ObjDetailsViewModel>());
            OpenFiles = new ProxyDialog<OpenFilesWindow, OpenFilesViewModel>(
                () => service.ResolveView<OpenFilesWindow, OpenFilesViewModel>());
        }

        public ProxyDialog<ObjDetailsWindow, ObjDetailsViewModel> ObjDetails { get; }
        public ProxyDialog<OpenFilesWindow, OpenFilesViewModel>  OpenFiles { get; }

        public string OpenFolderDialog(DirectoryInfo directory) {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.InitialDirectory = directory.FullName;
            dialog.Filter = "*.png|*.png";
            dialog.DefaultExt = "*.png|*.png";
            dialog.Title = "Open image";

            if (dialog.ShowDialog() == false) {
                return null;
            }

            return dialog.FileName;
        }
    }
}
