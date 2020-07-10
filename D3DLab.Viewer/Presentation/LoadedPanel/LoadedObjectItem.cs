using D3DLab.ECS;
using D3DLab.Viewer.D3D;
using D3DLab.Viewer.Presentation.Componets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation.LoadedPanel {
    
    class LoadedObjectItem : BaseNotify {
        public string Name => Visual.Description;
        public LoadedObjectDetails Details => Visual.Details;
        public bool IsVisible { get; set; }
        public bool IsBoundsShowed { get; set; }
        public FileInfo File { get; }

        public IViewComponent ActiveComponent { get => component; private set => Update(ref component, value); }
        IViewComponent component;


        public readonly LoadedVisualObject Visual;
        public LoadedObjectItem(LoadedVisualObject loaded, FileInfo file) {
            this.Visual = loaded;
            File = file;
            IsVisible = true;
        }
        public void ShowBoundingBox(IContextState context) {
            Visual.ShowBoundingBox(context, out var box);
            ActiveComponent = new BoundingBoxDetailsViewModel(box);
        }
        public void HideBoundingBox(IContextState context) {
            Visual.HideBoundingBox(context);
            ActiveComponent = null;
        }
        public void Transform(IEntityManager manager, in Matrix4x4 matrix) {
            Visual.Transform(manager, matrix);
            ActiveComponent?.ApplyMatrix(matrix);
        }
    }
}
