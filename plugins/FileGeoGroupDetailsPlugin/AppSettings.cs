using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Media;

namespace FileGeometryGroupsPlugin {
    [Serializable]
    public class ObjGroupFilter {
        public string Name { get; set; }
        public string Color { get; set; }
        public string Filter { get; set; }
    }
    class AppSettings {
        const string settingName = "filegroupsplugin.conf";

        readonly List<ObjGroupFilter> objGroupFilters;
        public AppSettings() {
            if (!File.Exists(settingName)) {
                objGroupFilters = new List<ObjGroupFilter>();
                Default(objGroupFilters);
                File.WriteAllText(settingName, JsonSerializer.Serialize(objGroupFilters));
            }

            objGroupFilters = JsonSerializer.Deserialize<List<ObjGroupFilter>>(File.ReadAllText(settingName));
        }

        public void AddNewObjGroupFilter(ObjGroupFilter filter) {
            objGroupFilters.Add(filter);

           File.WriteAllText(settingName, JsonSerializer.Serialize(objGroupFilters));
        }
        public IEnumerable<ObjGroupFilter> GetObjGroupFilters() => objGroupFilters;


        static void Default(List<ObjGroupFilter> objGroupFilters) {
            objGroupFilters.Add(new ObjGroupFilter { Filter = "^MUST_START ANY_WITH*?", Color = Colors.Blue.ToString() });
            objGroupFilters.Add(new ObjGroupFilter { Filter = "^((?!EXACT_VALUE).)*$", Color = Colors.Green.ToString() });
        }
    }
}
