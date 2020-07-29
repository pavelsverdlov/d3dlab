using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D3DLab.Viewer.Infrastructure {
    static class WindowsDefaultDialogs {
        [Flags]
        public enum FileFormats {
            Obj,
            Stl,

            MeshFormats = Obj | Stl,
            All,
        }
        public static string SaveFileDialog(DirectoryInfo directory, FileFormats formats) {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.InitialDirectory = directory.FullName;
            if (dialog.ShowDialog()!.Value) {
                return dialog.FileName;
            }
            return null;
        }
        public static string[] OpenFolderDialog(DirectoryInfo directory, FileFormats formats) {
            var formatsBuilder = new StringBuilder();

            if (formats.HasFlag(FileFormats.MeshFormats)) {
                formatsBuilder.Append("3D files (*.obj,*.stl )|*.obj;*.stl|");
            }

            if (formats.HasFlag(FileFormats.All)) {
                formatsBuilder.Append("All files (*.*)|*.*");
            }

            var filter = formatsBuilder.ToString().TrimEnd('|');

            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.InitialDirectory = directory.FullName;
            dialog.Multiselect = true;
            dialog.Filter = filter;
            dialog.DefaultExt = filter;
            dialog.Title = "Open file";

            if (dialog.ShowDialog() == false) {
                return null;
            }

            return dialog.FileNames;
        }
    }
}
