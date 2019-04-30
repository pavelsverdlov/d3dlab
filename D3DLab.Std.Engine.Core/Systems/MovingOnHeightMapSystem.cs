using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Movements;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace D3DLab.Std.Engine.Core.Systems {
    public interface IStickOnHeightMapComponent : IGraphicComponent {
        Ray GetRay();
    }
    public interface IHeightMapSourceComponent : IGraphicComponent {
        Matrix4x4 GetTransfromToMap(ref Ray ray, ref BoundingBox box);
    }
    public class MovingOnHeightMapSystem : BaseEntitySystem, IGraphicSystem {

        protected override void Executing(SceneSnapshot snapshot) {
            var emanager = snapshot.ContextState.GetEntityManager();
            var toProcess = new List<GraphicEntity>();
            IHeightMapSourceComponent source = null;
            foreach (var entity in emanager.GetEntities()) {
                if (entity.Has<IStickOnHeightMapComponent>()) {
                    toProcess.Add(entity);
                }
                var s = entity.GetComponents<IHeightMapSourceComponent>();
                if (s.Any()) {
                    source = s.First();
                }
            }

            if (source.IsNull()) {
                return;
            }

            foreach(var en in toProcess) {
                var p = en.GetComponent<IStickOnHeightMapComponent>();
                var box = en.GetComponent<IGeometryComponent>().Box;
                var tr = en.GetComponent<TransformComponent>();

                //var matrix = source.GetTransfrom(p);

              //  tr.MatrixWorld *= matrix;
            }

        }
    }
}
