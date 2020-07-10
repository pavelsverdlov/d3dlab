using D3DLab.Viewer.Properties;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace D3DLab.Viewer.Infrastructure {
    class AppSettings {
        static readonly ViewerSettings settings;
        static AppSettings() {
            settings = ViewerSettings.Default;
            settings.RecentFilePaths ??= new System.Collections.Specialized.StringCollection();
        }

        public IEnumerable<string> GetReceintFiles() {
            var list = new List<string>();
            foreach (var file in settings.RecentFilePaths) {
                list.Add(file);
            }
            return list;
        }

        public void SaveRecentFilePaths(string[] files) {
            var all = new HashSet<string>(GetReceintFiles());
            foreach (var p in files) {
                all.Add(p);
            }
            settings.RecentFilePaths.Clear();
            settings.RecentFilePaths.AddRange(all.ToArray());
            settings.Save();
        }
    }
}
