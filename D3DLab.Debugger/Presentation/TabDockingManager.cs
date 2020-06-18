using D3DLab.ECS.Shaders;
using D3DLab.Debugger.ECSDebug;
using D3DLab.Debugger.Presentation.TDI;
using D3DLab.Debugger.Presentation.TDI.ComponentList;
using D3DLab.Debugger.Presentation.TDI.Editer;
using D3DLab.Debugger.Presentation.TDI.Property;
using D3DLab.Debugger.Presentation.TDI.SystemList;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Debugger.Presentation {
    class DockingManagerTest : IDebugingTabDockingManager {
        public ObservableCollection<DockItem> Tabs { get; }

        public void OpenComponetsTab(BaseNotify mv) {

        }

        public void OpenPropertiesTab(IEditingProperties properties, IRenderUpdater updater) {

        }

        public void OpenSceneTab(BaseNotify mv) {

        }

        public void OpenShaderEditerTab(IShadersContainer mv, IRenderUpdater updater) {

        }

        public void OpenSystemsTab(BaseNotify mv) {

        }
    }
    class TabDockingManager : IDockingTabManager {
        public ObservableCollection<DockItem> Tabs { get; }

        readonly List<PropertyVeiwModel> propertyTabs;

        DockItem leftTabFixed;
        DockItem rightTabFixed;
        DockItem bottomTabFixed;

        public TabDockingManager() {
            propertyTabs = new List<PropertyVeiwModel>();
            Tabs = new ObservableCollection<DockItem>();
            InitDefaultTabs();
        }

       
        void InitDefaultTabs() {
            leftTabFixed = CreateLeft();
            leftTabFixed.Name = "LeftTab";
            leftTabFixed.Header = "Componets";
            ApplyFixed(leftTabFixed);
           // leftTabFixed.Content = new ComponetsUCTab { DataContext = main.Componets };
            

            //var sys = CreateLeft();
            //sys.Header = "Systems";
            //sys.SideInDockedMode = DockSide.Tabbed;
            //sys.TargetNameInDockedMode = leftTabFixed.Name;
            //ApplyFixed(sys);
            //sys.Content = new SystemsUCTab { DataContext = main.Systems };


            Tabs.Add(leftTabFixed);

            //

            rightTabFixed = CreateRight();
            rightTabFixed.Name = "RightTab";
            rightTabFixed.Header = "3D Objects";
            ApplyFixed(rightTabFixed);
            rightTabFixed.Content = new Button() { Content = "3D Objects" };


            Tabs.Add(rightTabFixed);
           

            //

            bottomTabFixed = CreateBottom();
            bottomTabFixed.Name = "BottomTab";
            bottomTabFixed.Header = "Output";
            ApplyFixed(bottomTabFixed);

            var interactive = CreateBottom();
            interactive.Header = "Interactive";
            interactive.SideInDockedMode = DockSide.Tabbed;
            interactive.TargetNameInDockedMode = bottomTabFixed.Name;
            ApplyFixed(interactive);

            Tabs.Add(bottomTabFixed);
            Tabs.Add(interactive);

            //
        }

        DockItem CreateLeft() {
            var item = new DockItem();          
            item.State = DockState.Dock;
            item.SideInDockedMode = DockSide.Left;
            item.DesiredWidthInDockedMode = 260;
            item.SizetoContentInDock = true;
            return item;
        }
        DockItem CreateRight() {
            var item = new DockItem();
            item.State = DockState.Dock;
            item.SideInDockedMode = DockSide.Right;
            item.DesiredWidthInDockedMode = 250;
            item.SizetoContentInDock = true;
            return item;
        }
        DockItem CreateBottom() {
            var item = new DockItem();
            item.State = DockState.Dock;
            item.SideInDockedMode = DockSide.Bottom;
            //item.DesiredWidthInDockedMode = ;
            item.SizetoContentInDock = true;
            return item;
        }
        DockItem CreateDocument() {
            var item = new DockItem();
            item.State = DockState.Document;
            item.SideInDockedMode = DockSide.Tabbed;
            //item.DesiredWidthInDockedMode = ;
            item.SizetoContentInDock = true;
            return item;
        }
        static void ApplyFixed(DockItem item) {
            //item.ShowPin = false;
            //item.IsPinned" Value="False"/>
            item.CanMaximize = false;
            item.ShowFloatingMenuItem = false;
            item.CanFloat = false;
            item.CanClose = false;
            item.IsContextMenuButtonVisible = false;
            item.CanDrag = false;
            item.DockAbility = DockAbility.None;
            item.SizetoContentInDock = false;
            item.CanAutoHide = false;
            //item.NoHeader = true;
        }


        public void OpenComponetsTab(BaseNotify mv) {
            leftTabFixed.Content = new ComponetsUCTab { DataContext = mv };
        }
        public void OpenSystemsTab(BaseNotify mv) {
            var sys = CreateLeft();
            sys.Header = "Systems";
            sys.SideInDockedMode = DockSide.Tabbed;
            sys.TargetNameInDockedMode = leftTabFixed.Name;
            ApplyFixed(sys);
            sys.Content = new SystemsUCTab { DataContext = mv };
            Tabs.Add(sys);
        }
        public void OpenSceneTab(BaseNotify mv) {
            var scene = CreateDocument();
            scene.Header = "Scene";
            scene.CanClose = true;
            scene.Content = new SceneWinFormUCTab() { DataContext = mv };

            Tabs.Add(scene);
        }

        public void OpenPropertiesTab(IEditingProperties properties, IRenderUpdater updater) {
            var property = CreateRight();
            property.Header = properties.Titile;
            property.SideInDockedMode = DockSide.Tabbed;
            property.TargetNameInDockedMode = rightTabFixed.Name;
            //ApplyFixed(property);
            property.CanClose = true;
            property.CanDrag = true;
            var vm = new PropertyVeiwModel(properties, updater);
            property.Content = new PropertyUCTab() { DataContext =vm  };
            propertyTabs.Add(vm);
            Tabs.Add(property);
        }

        public void OpenShaderEditerTab(IShadersContainer shader, IRenderUpdater updater) {
            var scene = CreateDocument();
            scene.Header = "Editer";
            scene.CanClose = true;
            scene.Content = new ShaderEditerUCTab() { DataContext = new ShaderEditerViewModel(shader, updater) };

            Tabs.Add(scene);
        }

        void IPropertyTabUpdater.Update() {
            foreach(var pr in propertyTabs) {
                pr.EditingProperties.Refresh();
            }
        }

        public void TabClosed(UserControl control) {
            //Tabs remove
            switch (control) {
                case PropertyUCTab property:
                    var item = Tabs.Single(x => x.Content == property);
                    Tabs.Remove(item);
                    propertyTabs.Remove((PropertyVeiwModel)property.DataContext);
                    break;

            }
        }
    }
}
