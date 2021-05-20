using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.ECS.Ext;
using D3DLab.FileFormats.GeoFormats;
using D3DLab.Plugin;
using D3DLab.Toolkit;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Math3D;

using WPFLab;
using WPFLab.MVVM;

namespace FileGeometryGroupsPlugin {
    class FileViewModel {
        public IPluginLoadedObjectDetails Details { get; }
        public FileViewModel(IPluginLoadedObjectDetails f) {
            this.Details = f;
        }
    }

    class MainViewModel : BaseNotify, IPluginViewModel {
        public class ColorFilterViewItem {
            private string filter;
            private string color;

            public string Filter {
                get => filter;
                set {
                    if (string.IsNullOrWhiteSpace(value)) { return; }
                    filter = value;
                }
            }
            public string Color {
                get => color;
                set {
                    if (string.IsNullOrWhiteSpace(value)) { return; }
                    color = value;
                }
            }
            public bool IsChecked { get; set; }
        }
        public class ObjGroupViewItem : BaseNotify {
            public string Name { get; }
            public int Count { get; }
            public bool IsVisible {
                get => isVisible;
                set {
                    isVisible = value;
                    SetPropertyChanged(nameof(IsVisible));
                }
            }
            public string Color {
                get => color;
                set {
                    color = value;
                    SetPropertyChanged(nameof(Color));
                }
            }

            public readonly ElementTag Entity;
            private bool isVisible;
            private string color;

            public ObjGroupViewItem(ElementTag entity, string name, string color, int duplicates) {
                Count = duplicates;
                Entity = entity;
                IsVisible = true;
                Name = name;
                Color = color;
            }
        }

        public ICollectionView Files { get; }
        public int ItemsCount { get; set; }
        public int AllGroupsCount { get; set; }

        public string Filter {
            get => filter;
            set {
                filter = value;
                OnFilter(filter);
            }
        }
        string filter;

        public ICollectionView FilterColors { get; }
        public Controler Controler { get; }
        public ICollectionView ObjGroups { get; private set; }
        ObservableCollection<ObjGroupViewItem> items;
        readonly ObservableCollection<ColorFilterViewItem> filterColors;
        ListCollectionView listView;

        readonly IPluginContext pluginContext;
        readonly AppSettings settings;
        readonly ObservableCollection<FileViewModel> files;

        public MainViewModel(IPluginContext context) {
            settings = new AppSettings();
            this.pluginContext = context;

            files = new ObservableCollection<FileViewModel>();
            Files = CollectionViewSource.GetDefaultView(files);

            Controler = new Controler(this);
            filterColors = new ObservableCollection<ColorFilterViewItem>();
            FilterColors = CollectionViewSource.GetDefaultView(filterColors);

            foreach (var f in settings.GetObjGroupFilters()) {
                AddNewColorFilter(f.Filter, (Color)ColorConverter.ConvertFromString(f.Color));
            }

            Files.CurrentChanged += Files_CurrentChanged;

            items = new ObservableCollection<ObjGroupViewItem>();
            ObjGroups = CollectionViewSource.GetDefaultView(items);
            listView = (ListCollectionView)ObjGroups;
        }

        void Files_CurrentChanged(object sender, EventArgs e) {
            var file = (FileViewModel)Files.CurrentItem;
            if (file == null) {
                return;
            }
            var context = pluginContext.Scene.Context;
            var comManager = pluginContext.Scene.Context.GetComponentManager();

            items.Clear();

            foreach (var tag in file.Details.VisualObjectTags) {
                if (TryGetMesh(context, tag, out var mesh)) {
                    var color = comManager.GetComponent<MaterialColorComponent>(tag).Diffuse;
                    var item = new ObjGroupViewItem(tag, mesh.OriginGeometry.Name, color.ToColor().ToHexString(), 1);
                    items.Add(item);
                }
            }

            if (!items.Any()) {
                return;
            }

            ObjGroups.SortDescriptions.Add(new SortDescription(nameof(ObjGroupViewItem.Count), ListSortDirection.Descending));

            
            SetPropertyChanged(nameof(ObjGroups));

            UpdateCount();
            //Refresh();
            ObjGroups.Refresh();
        }
        static bool TryGetMesh(IContextState context, in ElementTag tag, out GeometryStructures<IFileGeometry3D> structures) {
            var id = context.GetComponentManager().GetComponent<GeometryPoolComponent>(tag);
            structures = context.GetGeometryPool().GetGeometry<IGeometryData>(id) as GeometryStructures<IFileGeometry3D>;
            return structures != null;
        }


        public void Init() {
            foreach (var f in pluginContext.Objects) {
                var filevm = new FileViewModel(f);
                files.Add(filevm);
            }
        }

        public void Closed() {
            
        }

        internal void ShowHideAll(bool show) {
            foreach (var item in items) {
                var en = pluginContext.Scene.Context.GetEntityManager().GetEntity(item.Entity);
                var com = en.GetComponent<RenderableComponent>();

                en.UpdateComponent(show ? com.Enable() : com.Disable());

                item.IsVisible = show;
            }
        }
        internal void ShowHideItem(ObjGroupViewItem item, bool show) {
            var en = pluginContext.Scene.Context.GetEntityManager().GetEntity(item.Entity);
            var com = en.GetComponent<RenderableComponent>();

            en.UpdateComponent(show ? com.Enable() : com.Disable());
        }
        internal void AddNewColorFilter(string filer, Color color) {
            filterColors.Add(new ColorFilterViewItem {
                Color = color.ToString(),
                Filter = filer,
            });
        }
        internal void RemoveSelectedColor() {
            var checkedColors = filterColors.Where(x => x.IsChecked).ToList();
            foreach (var color in checkedColors) {
                filterColors.Remove(color);
            }
        }

        internal void Refresh() {
            Refresh(items);
        }

        void Refresh(IEnumerable<ObjGroupViewItem> groups) {
            var regex = new Tuple<Regex, Vector4, string>[filterColors.Count];
            for (var i = 0; i < filterColors.Count; i++) {
                var item = filterColors[i];
                var color = (Color)ColorConverter.ConvertFromString(item.Color);
                regex[i] = Tuple.Create(
                    new Regex(item.Filter, RegexOptions.Compiled),
                    new Vector4(color.ScR, color.ScG, color.ScB, color.ScA),
                    item.Color);
            }
            //keep the order of regex
            foreach (var reg in regex) {
                foreach (var item in groups) {
                    var match = reg.Item1.IsMatch(item.Name);
                    if (match) {
                        ChangeColor(item, reg.Item2);
                        item.Color = reg.Item3;
                    }
                }
            }

        }

        void ChangeColor(ObjGroupViewItem item, Vector4 color) {
            pluginContext.Scene.Context
                .GetEntityManager()
                .GetEntity(item.Entity)
                .UpdateComponent(MaterialColorComponent.Create(color));
        }


        void OnFilter(string filter) {
            if (ObjGroups.IsNull()) { return; }
            if (string.IsNullOrWhiteSpace(filter)) {
                ObjGroups.Filter = x => true;
            } else {
                try {
                    var f = new Regex(filter, RegexOptions.Compiled);
                    ObjGroups.Filter = x => {
                        var item = (ObjGroupViewItem)x;
                        return f.IsMatch(item.Name);
                    };
                } catch {
                    ObjGroups.Filter = x => {
                        var item = (ObjGroupViewItem)x;
                        return item.Name.Contains(filter);
                    };
                }
            }
            UpdateCount();
        }

        void UpdateCount() {
            ItemsCount = listView.Count;
            SetPropertyChanged(nameof(ItemsCount));

            var count = 0;
            foreach (var i in listView) {
                count += ((ObjGroupViewItem)i).Count;
            }
            AllGroupsCount = count;
            SetPropertyChanged(nameof(AllGroupsCount));
        }
    }


    public class ObjGroup {
        public readonly string Name;
        public ObjGroup(string name) {
            Name = name;
        }
    }
    class Controler {
        public ICommand Refresh { get; }

        public ICommand CopyGroupName { get; }
        public ICommand VisiblityChanged { get; }

        public ICommand AddNewColorFilter { get; }
        public ICommand RemoveColorFilter { get; }

        public ICommand ShowHideGroup { get; }
        public ICommand HighlightGroup { get; }
        //
        public ICommand ShowHideAllCommand { get; }

        readonly MainViewModel facade;

        public Controler(MainViewModel facade) {
            this.facade = facade;
            ShowHideGroup = new WpfActionCommand<bool>(OnShowHideGroup);
            HighlightGroup = new WpfActionCommand<bool>(OnHighlightGroup);
            AddNewColorFilter = new WpfActionCommand(OnAddNewColorFilter);
            RemoveColorFilter = new WpfActionCommand(OnRemoveColorFilter);

            Refresh = new WpfActionCommand(OnRefresh);
            CopyGroupName = new WpfActionCommand<string>(OnWpfActionCommand);
            VisiblityChanged = new WpfActionCommand<MainViewModel.ObjGroupViewItem>(OnVisiblityChanged);
            ShowHideAllCommand = new WpfActionCommand<bool>(OnShowHideAll);
        }

        void OnShowHideAll(bool isChecked) {
            facade.ShowHideAll(isChecked);
        }

        void OnVisiblityChanged(MainViewModel.ObjGroupViewItem item) {
            facade.ShowHideItem(item, item.IsVisible);
        }

        void OnWpfActionCommand(string name) {
            facade.AddNewColorFilter(name, Colors.White);
        }

        void OnRefresh() {
            facade.Refresh();
        }

        void OnAddNewColorFilter() {
            facade.AddNewColorFilter(null, Colors.White);
        }
        void OnRemoveColorFilter() {
            facade.RemoveSelectedColor();
        }
        void OnHighlightGroup(bool obj) {

        }
        void OnShowHideGroup(bool show) {

        }
    }
}
