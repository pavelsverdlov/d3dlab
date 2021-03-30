using D3DLab.Viewer.Infrastructure.Settings;
using D3DLab.Viewer.Properties;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Media;

namespace D3DLab.Viewer.Infrastructure {
    class AppSettings {
        static readonly ViewerSettings settings;
        static AppSettings() {
            settings = ViewerSettings.Default;
            settings.RecentFilePaths ??= new System.Collections.Specialized.StringCollection();

           
        }

        readonly List<ObjGroupFilter> objGroupFilters;
        public AppSettings() {
            if (settings.ObjGroupFilters.Length == 0) {
                objGroupFilters = new List<ObjGroupFilter>();
                Default(objGroupFilters);
                settings.ObjGroupFilters = JsonSerializer.Serialize(objGroupFilters);
            } else {
                objGroupFilters = JsonSerializer.Deserialize<List<ObjGroupFilter>>(settings.ObjGroupFilters);
            }
        }

        public IEnumerable<string> GetReceintFiles() {
            var list = new List<string>();
            foreach (var file in settings.RecentFilePaths) {
                if (File.Exists(file)) {
                    list.Add(file);
                }
            }
            return list;
        }
        public void SaveRecentFilePaths(string[] files) {
            var all = new HashSet<string>(GetReceintFiles());
            foreach (var p in files) {
                if (File.Exists(p)) {
                    all.Add(p);
                }
            }
            settings.RecentFilePaths.Clear();
            settings.RecentFilePaths.AddRange(all.ToArray());
            settings.Save();
        }
        public void ClearReceintFiles() {
            settings.RecentFilePaths.Clear();
            settings.Save();
        }


        public void AddNewObjGroupFilter(ObjGroupFilter filter) {
            objGroupFilters.Add(filter);

            settings.ObjGroupFilters = JsonSerializer.Serialize(objGroupFilters);
            settings.Save();
        }
        public IEnumerable<ObjGroupFilter> GetObjGroupFilters() => objGroupFilters;


        static void Default(List<ObjGroupFilter> objGroupFilters) {
            objGroupFilters.Add(new ObjGroupFilter { Filter = "^MUST_START ANY_WITH*?", Color = Colors.Blue.ToString() });
            objGroupFilters.Add(new ObjGroupFilter { Filter = "^((?!EXACT_VALUE).)*$", Color = Colors.Green.ToString() });
        }
    }
}
