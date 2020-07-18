using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.Toolkit.Components;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Viewer.D3D.Systems {
    public struct ZoomToAllCompponent : IGraphicComponent {
        public static ZoomToAllCompponent Create() =>
            new ZoomToAllCompponent(ElementTag.New());

        public ElementTag Tag { get; }
        public bool IsValid => true;
        public ZoomToAllCompponent(ElementTag elementTag) : this() {
            this.Tag = elementTag;
        }
        public void Dispose() {

        }
    }
    class ZoomToAllObjectsSystem : BaseEntitySystem, IGraphicSystem, IGraphicSystemContextDependent {
        public IContextState ContextState { set; private get; }

        protected override void Executing(ISceneSnapshot snapshot) {
            var emanager = ContextState.GetEntityManager();

            var world = emanager.GetEntity(snapshot.WorldTag);

            if (!world.Contains<ZoomToAllCompponent>()) {
                return;
            }

            var fullBox = new AxisAlignedBox();
            foreach (var entity in emanager.GetEntities()) {
                if(entity.TryGetComponents<GeometryBoundsComponent, TransformComponent>(out var box, out var tr)) {
                    fullBox = fullBox.Merge(box.Bounds.Transform(tr.MatrixWorld));
                }                
            }

            var surface = snapshot.Surface.Size;
            var aspectRatio = surface.Width / surface.Height;

            var size = fullBox.Size();

            var camera = ContextState.GetEntityManager().GetEntity(snapshot.CurrentCameraTag);
            var com = OrthographicCameraComponent.Clone(camera.GetComponent<OrthographicCameraComponent>());

            var move = Math.Max(Math.Abs(com.LookDirection.X * size.X),
                Math.Max(Math.Abs(com.LookDirection.Y * size.Y), Math.Abs(com.LookDirection.Z * size.Z)));

            com.Position = fullBox.Center + com.LookDirection * -move * 10;
            com.RotatePoint = fullBox.Center;
            com.Width = Math.Max(size.X, Math.Max(size.Y, size.Z)) * aspectRatio;
            com.Scale = 1;

            camera.UpdateComponent(com);

            world.RemoveComponent<ZoomToAllCompponent>();
        }
    }
}
