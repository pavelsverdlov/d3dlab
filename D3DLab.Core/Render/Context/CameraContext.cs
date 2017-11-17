using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core.Render.Camera;
using HelixToolkit.Wpf.SharpDX.Render;
using HelixToolkit.Wpf.SharpDX.WinForms;
using SharpDX;

namespace D3DLab.Core.Render.Context {
    public class CameraContext {
        public Vector3 CameraPosition { get; private set; }
        public Vector3 CameraLookDirection { get; private set; }
        public Vector3 CameraUpDirection { get; private set; }
        public double ActualWidth { get; private set; }
        public double ActualHeight { get; private set; }


        public CameraD3D Camera { get; private set; }

        public bool isProjCamera;
        public Vector4 viewport, frustum;

        public Matrix worldMatrix = Matrix.Identity;
        public Matrix viewMatrix;
        public Matrix projectionMatrix;

        public Matrix ViewMatrix { get { return this.viewMatrix; } }
        public Matrix ProjectrionMatrix { get { return this.projectionMatrix; } }
       // public float PixelSize { get; private set; }


        /// <summary>
        /// need set camera before each render for actual camera state
        /// </summary>
        public void SetCamera(ISharpRenderTarget RenderTarget, CameraD3D camera) {
            Camera = camera;
            CameraPosition = Camera.Position;
            CameraLookDirection = Camera.LookDirection;
            CameraUpDirection = Camera.UpDirection;

            //var ortho = camera as OrthographicCamera;
            this.viewMatrix = this.Camera.CreateViewMatrix();
            ActualWidth = RenderTarget.Width;
            ActualHeight = RenderTarget.Height;

            var orphCamera = Camera as OrthographicCamera;
        //    PixelSize = orphCamera != null ? (float)(orphCamera.Width / ActualWidth) : 1f;

            var aspectRatio = ActualWidth / RenderTarget.Height;
            this.projectionMatrix = this.Camera.CreateProjectionMatrix(aspectRatio);

            var projCamera = this.Camera as ProjectionCamera;
            isProjCamera = projCamera != null;
           /* if (projCamera != null) {
                // viewport: W,H,0,0
                this.viewport = new Vector4((float)RenderTarget.Width, (float)RenderTarget.Height, 0, 0);
                var ar = viewport.X / viewport.Y;

                var pc = projCamera as PerspectiveCamera;
                var fov = (pc != null) ? pc.FieldOfView : 90f;

                var zn = projCamera.NearPlaneDistance > 0 ? projCamera.NearPlaneDistance : 0.1;
                var zf = projCamera.FarPlaneDistance + 0.0;
                // frustum: FOV,AR,N,F
                this.frustum = new Vector4((float)fov, (float)ar, (float)zn, (float)zf);
            }*/
        }

        public void CopyCamera(CameraD3D camera) {
            //            if (context.Camera == null) {
            //                context.Camera = null;
            //                return;
            //            }

            var renderCamera = camera;
            if (Camera == null || camera.GetType() != Camera.GetType())
                renderCamera = (CameraD3D)Activator.CreateInstance(camera.GetType());

            var srcOrthCamera = Camera as OrthographicCamera;
            if (srcOrthCamera != null) {
                var renderOrthCamera = (OrthographicCamera)renderCamera;
                renderOrthCamera.Width = srcOrthCamera.Width;
                renderOrthCamera.Position = srcOrthCamera.Position;
                renderOrthCamera.LookDirection = srcOrthCamera.LookDirection;
                renderOrthCamera.UpDirection = srcOrthCamera.UpDirection;
                renderOrthCamera.CreateLeftHandSystem = srcOrthCamera.CreateLeftHandSystem;
                renderOrthCamera.FarPlaneDistance = srcOrthCamera.FarPlaneDistance;
                renderOrthCamera.NearPlaneDistance = srcOrthCamera.NearPlaneDistance;
            }

            Camera = renderCamera;
        }

        public void RenderCamera(EffectContext context) {
            context.Effect.Variables().EyePos.Set(CameraPosition);
            context.Effect.Variables().Projection.SetMatrix(ref projectionMatrix);
            context.Effect.Variables().View.SetMatrix(ref viewMatrix);
            if (isProjCamera) {
                context.Effect.Variables().Viewport.Set(ref viewport);
                context.Effect.Variables().Frustum.Set(ref frustum);
                context.Effect.Variables().EyeLook.Set(CameraLookDirection);
            }
        }
    }
}
