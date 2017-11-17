using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using D3DLab.Core.Components;
using D3DLab.Core.Entities;
using SharpDX;
using Matrix = SharpDX.Matrix;

namespace D3DLab.Core {
    public sealed class SceneData {
        public IViewportControl Viewport { get; set; }
    }

    public class BaseScene : Entity<SceneData> {
        public BaseScene() : base("Scene") {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector">cursor point</param>
        /// <returns></returns>
        public Ray ConvertToRay(Vector2 vector) {
            var camera = GetComponents<ICameraComponent>();
            if (!camera.Any()) {
                throw new Exception("No camera");
            }
            var vector3 = new Vector3(vector.X, vector.Y, 0);
            var matrixViewport = Data.Viewport.GetViewportTransform();
            var matrixCamera = camera.Single().Data.GetFullMatrix((float)Data.Viewport.Width / Data.Viewport.Height);

            matrixViewport.Invert();
            matrixCamera.Invert();

            var pointNormalized = Vector3.TransformCoordinate(vector3,matrixViewport);
            pointNormalized.Z = 0.01f;
            var pointNear = Vector3.TransformCoordinate(pointNormalized, matrixCamera);
            pointNormalized.Z = 0.99f;
            var pointFar = Vector3.TransformCoordinate(pointNormalized,matrixCamera);

            return new SharpDX.Ray(pointNear, pointFar);
        }

        public Ray UnProject(Vector2 point2d) {
            var camera = GetComponents<ICameraComponent>().Single();
            var p = new Vector3(point2d.X, point2d.Y, 1);

            var matrixViewport = Data.Viewport.GetViewportTransform();
            var matrixCamera = camera.Data.GetFullMatrix((float)Data.Viewport.Width / Data.Viewport.Height);

            var vpi = matrixCamera * matrixViewport;
            vpi.Invert();

            Vector3 zn, zf;
            p.Z = 0;
            Vector3.TransformCoordinate(ref p, ref vpi, out zn);
            p.Z = 1f;
            Vector3.TransformCoordinate(ref p, ref vpi, out zf);
            Vector3 r = zf - zn;
            r.Normalize();

            if (double.IsNaN(zn.X) || double.IsNaN(zn.Y) || double.IsNaN(zn.Z)) {
                zn = new Vector3(0, 0, 0);
            }
            if (double.IsNaN(r.X) || double.IsNaN(r.Y) || double.IsNaN(r.Z) ||
                (r.X == 0 && r.Y == 0 && r.Z == 0)) {
                r = new Vector3(0, 0, 1);
            }
            //fix for not valid inverted matrix
            return new Ray(zn, r);
        }
        public Ray Point2DtoRay3D(Vector2 pointIn) {
            Vector3 pointNear, pointFar;
            //if (!Point2DtoPoint3D(pointIn, out pointNear, out pointFar)) {
                var camera = GetComponents<ICameraComponent>().Single();
                var matrixViewport = Data.Viewport.GetViewportTransform();
                var matrixCamera = camera.Data.GetFullMatrix((float)Data.Viewport.Width / Data.Viewport.Height);
                var test =  Ray.GetPickRay((int) pointIn.X, (int) pointIn.Y,new ViewportF(0,0, Data.Viewport.Width, Data.Viewport.Height), matrixCamera);
                return test;
            //}

            return new Ray(pointNear, pointFar);
        }

        private bool Point2DtoPoint3D(Vector2 pointIn, out Vector3 pointNear, out Vector3 pointFar) {
            var camera = GetComponents<ICameraComponent>().Single();
            pointNear = new Vector3();
            pointFar = new Vector3();

            var pointIn3D = new Vector3(pointIn.X, pointIn.Y, 0);
            var matrixViewport = Data.Viewport.GetViewportTransform();
            var matrixCamera = camera.Data.GetFullMatrix((float)Data.Viewport.Width / Data.Viewport.Height);
            
            matrixViewport.Invert();
            matrixCamera.Invert();

            Vector3 pointNormalized;
            Vector3.TransformCoordinate(ref pointIn3D, ref matrixViewport, out pointNormalized);
            pointNormalized.Z = 0.01f;
            
            Vector3.TransformCoordinate(ref pointNormalized, ref matrixCamera, out pointNear);
            pointNormalized.Z = 0.99f;
            
            Vector3.TransformCoordinate(ref pointNormalized, ref matrixCamera, out pointFar);

            return true;
        }

        /*
        public void UpdateBackgroundColor(IRenderHost renderHost) {
            var backGroundRenderData = new MeshGeometryRenderData(Techniques.RenderBackground);
            var mesh = new MeshGeometry3D();


            var bitmap = GenerateBackGroundBitmapSource(Background, 100, 100);
            mesh.Positions = new Vector3Collection() {
                    new Vector3(-1, -1, 1),
                    new Vector3(1, -1, 1),
                    new Vector3(-1, 1, 1),
                    new Vector3(1, 1, 1)
                }; //

            mesh.Indices = new IntCollection() { 0, 1, 2, 1, 3, 2 };
            mesh.TextureCoordinates = new Vector2Collection() {
                    new Vector2(0, 1),
                    new Vector2(1, 1),
                    new Vector2(0, 0),
                    new Vector2(1, 0)
                };
            backGroundRenderData.RenderSources.Positions.Update(mesh);
            backGroundRenderData.RenderSources.Indices.Update(mesh);
            //backGroundRenderData.RenderSources.Colors.Update(mesh);
            backGroundRenderData.RenderSources.TextureCoordinates.Update(mesh);
            backGroundRenderData.RenderTechniqueUserDefinded = Techniques.RenderBackground;
            backGroundRenderData.Material = new PhongMaterial() { DiffuseMap = bitmap };
            backGroundRenderData.Visible = true;
            backGroundRenderData.TextureCoordScale = new Vector2(1);

            backGroundRenderData.Transform = global::SharpDX.Matrix.Identity;
            backGroundRenderData.Attach();
            //            } else {
            //                var bitmap = GenerateBackGroundBitmapSource(Background, 100, 100);
            //                backGroundRenderData.Material = new PhongMaterial() { DiffuseMap = bitmap };
            //            }
        }*/

        private static BitmapSource GenerateBackGroundBitmapSource(Brush brush, int height, int width) {

            var bmp = new RenderTargetBitmap(width, (int)(height), 96, 96, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();
            using (var ctx = drawingVisual.RenderOpen()) {
                ctx.DrawRectangle(brush, null, new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            }
            bmp.Render(drawingVisual);

            return bmp;
        }

     
        public override void Dispose() {
       
        }
        
    }

}
