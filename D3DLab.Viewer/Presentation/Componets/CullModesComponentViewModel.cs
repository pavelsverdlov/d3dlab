using D3DLab.ECS;
using D3DLab.Viewer.D3D;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Windows.Input;

using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation.Componets {

    class CullModesComponentViewModel : BaseNotify, IViewComponent {
        readonly LoadedVisualObject visual;
        readonly IContextState context;
        bool isFrontModeEnable;
        bool isBackModeEnable;
        bool isDoubleModeEnable;

        public bool IsFrontModeEnable {
            get => isFrontModeEnable; 
            set { 
                Update(ref isFrontModeEnable, value);
            } 
        }
        public bool IsBackModeEnable { 
            get => isBackModeEnable;
            set { 
                Update(ref isBackModeEnable, value);
            }
        }
        public bool IsDoubleModeEnable {
            get => isDoubleModeEnable;
            set { 
                Update(ref isDoubleModeEnable, value);
            }
        }
        public ICommand FrontModeEnableCommand { get; }
        public ICommand BackModeEnableCommand { get; }
        public ICommand DoubleModeEnableCommand { get; }

        public CullModesComponentViewModel(LoadedVisualObject visual, IContextState context) {
            this.visual = visual;
            this.context = context;

            FrontModeEnableCommand = new WpfActionCommand(OnFrontEnabled);
            BackModeEnableCommand = new WpfActionCommand(OnBackEnabled);
            DoubleModeEnableCommand = new WpfActionCommand(OnDoubleEnabled);

            switch (visual.CullMode) {
                case SharpDX.Direct3D11.CullMode.Front:
                    IsFrontModeEnable= true;
                    break;
                case SharpDX.Direct3D11.CullMode.Back:
                    IsBackModeEnable = true;
                    break;
                case SharpDX.Direct3D11.CullMode.None:
                    IsDoubleModeEnable = true;
                    break;
            }
        }


        public void ApplyMatrix(in Matrix4x4 matrix) {

        }

        void OnFrontEnabled() {
            if (IsFrontModeEnable) {
                SetPropertyChanged(nameof(IsFrontModeEnable));
                return;
            }
            IsBackModeEnable = IsDoubleModeEnable = false;
            IsFrontModeEnable = true;
            visual.ChangeCullMode(context, SharpDX.Direct3D11.CullMode.Front);
        }
        void OnBackEnabled() {
            if (IsBackModeEnable) {
                SetPropertyChanged(nameof(IsBackModeEnable));
                return;
            }
            IsFrontModeEnable = IsDoubleModeEnable = false;
            IsBackModeEnable = true;
            visual.ChangeCullMode(context, SharpDX.Direct3D11.CullMode.Back);
        }
        void OnDoubleEnabled() {
            if (IsDoubleModeEnable) {
                SetPropertyChanged(nameof(IsDoubleModeEnable));
                return;
            }
            IsFrontModeEnable = IsBackModeEnable = false;
            IsDoubleModeEnable = true;
            visual.ChangeCullMode(context, SharpDX.Direct3D11.CullMode.None);
        }
    }
}
