using D3DLab.Core.Components;
using D3DLab.Core.Entities;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using Component = D3DLab.Core.Components.Component;

namespace D3DLab.Core.Render.Components {
    public sealed class OrthographicCameraRenderComponent : Component, IRenderComponent, IAttachTo<OrthographicCameraEntity> {
        private struct RenderData {
            public Matrix ViewMatrix;
            public Matrix ProjectionMatrix;
            public Vector3 Position;
            public Vector3 LookDirection;
        }
        private RenderData data;
        
        public void Update(Graphics graphics) {
            base.Update();
            var aspectRatio = (float)graphics.SharpDevice.Width / graphics.SharpDevice.Height; 
             var rd = Parent.Data;
            data = new RenderData {
                Position = rd.Position,
                LookDirection = rd.LookDirection,
                ViewMatrix = rd.CreateViewMatrix(),
                ProjectionMatrix = rd.CreateProjectionMatrix(aspectRatio)
            };
            // var PixelSize = (float)(camera.Width / control.Width);
        }

        public void Render(World world, Graphics graphics) {
            var projectionMatrix = data.ProjectionMatrix;
            var viewMatrix = data.ViewMatrix;
            var viewport= Vector4.Zero;
            var frustum = Vector4.Zero;
            var variables = graphics.Variables(Techniques.RenderPhong);
            variables.EyePos.Set(data.Position);
            variables.Projection.SetMatrix(ref projectionMatrix);
            variables.View.SetMatrix(ref viewMatrix);
            //if (isProjCamera) {
            variables.Viewport.Set(ref viewport);
            variables.Frustum.Set(ref frustum);
            variables.EyeLook.Set(data.LookDirection);

//            Debug.WriteLine("{0} Render {1}", Thread.CurrentThread.ApartmentState, Parent.LookDirection);
        }

        public OrthographicCameraEntity Parent { get; private set; }

        public void OnAttach(OrthographicCameraEntity camera) {
            this.Parent = camera;
        }
    }
}