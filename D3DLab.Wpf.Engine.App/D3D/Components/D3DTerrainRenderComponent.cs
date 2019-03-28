using System.Collections.Generic;
using System.Numerics;
using D3DLab.SDX.Engine.Components;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using SharpDX.Direct3D11;

namespace D3DLab.Wpf.Engine.App.D3D.Components {
    
    public class D3DTerrainCellRenderComponent : D3DRenderComponent {
        public int IndexCount { get; set; }
    }

    public class TerrainGeometryComponent : HittableGeometryComponent {
        public Vector2[] NormalMapTexCoordinates { get; set; }
    }


    public class D3DTerrainRenderComponent : D3DRenderComponent {
        [IgnoreDebuging]
        internal EnumerableDisposableSetter<ShaderResourceView[]> TextureResources { get; set; }
        [IgnoreDebuging]
        internal DisposableSetter<SamplerState> SampleState { get; set; }

        public int CellCount => cells.Count;
        readonly List<D3DTerrainCellRenderComponent> cells;

        public D3DTerrainRenderComponent() {
            cells = new List<D3DTerrainCellRenderComponent>();
            SampleState = new DisposableSetter<SamplerState>(disposer);
            TextureResources = new EnumerableDisposableSetter<ShaderResourceView[]>(disposer);
        }

        public override void Dispose() {
            base.Dispose();
        }

        public D3DTerrainCellRenderComponent GetCell(int index, Frustum frustum) {
            var cell = cells[index];
            //if (frustum.CheckRectangle()) {

            //}
            return cell;
        }

    }
}
