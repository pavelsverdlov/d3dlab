using D3DLab.ECS;
using D3DLab.Viewer.D3D;
using D3DLab.Viewer.Presentation.LoadedPanel;

using SharpDX.Direct3D11;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Windows.Input;

using WPFLab;

namespace D3DLab.Viewer.Presentation.Componets {
    class WireframeComponetViewModel : IViewComponent {
        readonly LoadedVisualObject visual;
        readonly IContextState context;

        public bool IsTransparent { get; set; }
        public ICommand TransparentCheckedCommand { get; }
        public WireframeComponetViewModel(LoadedVisualObject visual, IContextState context) {
            this.visual = visual;
            this.context = context;
            IsTransparent = true;
            TransparentCheckedCommand = new WpfActionCommand(OnTransparentChecked);
        }

        void OnTransparentChecked() {
            if (IsTransparent) {
                visual.TurnSolidWireframeOff(context);
                visual.TurnTransparentWireframeOn(context);
            } else {
                visual.TurnTransparentWireframeOff(context);
                visual.TurnSolidWireframeOn(context);
            }
        }

        public void ApplyMatrix(in Matrix4x4 matrix) {
            
        }
    }
}
