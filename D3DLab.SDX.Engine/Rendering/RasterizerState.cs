using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.SDX.Engine.Rendering {
    public class D3DRasterizerState {
        public FillMode FillMode {
            get => state.FillMode;
            set {
                state.FillMode = value;
            }
        }
        public CullMode CullMode {
            get => state.CullMode;
            set {
                state.CullMode = value;
            }
        }
        RasterizerStateDescription state;
        public D3DRasterizerState(RasterizerStateDescription state) {
            this.state = state;
        }
        public RasterizerStateDescription GetDescription() {
            return state;
        }
    }
}
