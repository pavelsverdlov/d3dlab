using D3DLab.ECS;
using D3DLab.ECS.Context;
using D3DLab.Viewer.D3D;
using D3DLab.Viewer.Presentation.Componets;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using System.Text;

using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation.LoadedPanel {
    interface IWireframeOperations {
        void TurnTransparentWireframeOff();
    }
    class LoadedObjectItem : BaseNotify {
        public string Name => Visual.Description;
        public LoadedObjectDetails Details => Visual.Details;
        public bool IsVisible { get; set; }
        public bool IsBoundsShowed { get; set; }
        public bool IsFlatshadingEnabled { get; set; }
        public bool IsWireframeEnabled { get; set; }
        public FileInfo File { get; }

        public ObservableCollection<IViewComponent> ActiveComponents { get; }

        public readonly LoadedVisualObject Visual;

        BoundingBoxDetailsViewModel? boxComponent;
        WireframeComponetViewModel? wireframeComponent;
        CullModesComponentViewModel? cullComponent;

        public LoadedObjectItem(LoadedVisualObject loaded, FileInfo file) {
            this.Visual = loaded;
            File = file;
            IsVisible = true;
            ActiveComponents = new ObservableCollection<IViewComponent>();
            
        }

        public void ActivateStaticComponents(IContextState context) {
            if (cullComponent != null) throw new InvalidOperationException();

            cullComponent = new CullModesComponentViewModel(Visual, context);
            ActiveComponents.Add(cullComponent);
        }
        public void DeactivateStaticComponents() {
            if (cullComponent == null) throw new InvalidOperationException();

            ActiveComponents.Remove(cullComponent);
            cullComponent = null;
        }

        public void ShowBoundingBox(IContextState context) {
            Visual.ShowBoundingBox(context, out var box);
            boxComponent = new BoundingBoxDetailsViewModel(box);
            ActiveComponents.Add(boxComponent);
        }
        public void HideBoundingBox(IContextState context) {
            if (boxComponent == null) throw new InvalidOperationException();
            Visual.HideBoundingBox(context);
            ActiveComponents.Remove(boxComponent);
            boxComponent = null;
        }

        public void HideWireframe(IContextState context) {
            if (wireframeComponent == null) throw new InvalidOperationException();
            if (wireframeComponent.IsTransparent) {
                Visual.TurnTransparentWireframeOff(context);
            } else {
                Visual.TurnSolidWireframeOff(context);
            }
            ActiveComponents.Remove(wireframeComponent);
            wireframeComponent = null;
        }
        public void ShowWireframe(IContextState context) {
            Visual.TurnTransparentWireframeOn(context);
            wireframeComponent = new WireframeComponetViewModel(Visual, context);
            ActiveComponents.Add(wireframeComponent);
        }
        
        public void Transform(IEntityManager manager, in Matrix4x4 matrix) {
            Visual.Transform(manager, matrix);
            boxComponent?.ApplyMatrix(matrix);
        }
        
        public void HideFlatshadingMode(IContextState context) {
            Visual.TurnFlatshadingOff(context);
        }
        public void ShowFlatshadingMode(IContextState context) {
            Visual.TurnFlatshadingOn(context);
        }

        public void Refresh(IContextState context) {
            ActiveComponents.Clear();
            if (IsBoundsShowed) {
                ShowBoundingBox(context);
            }
            if (IsFlatshadingEnabled) {
                ShowFlatshadingMode(context);
            }
            if (IsWireframeEnabled) {
                ShowWireframe(context);
            }
        }

       
    }
}
