using D3DLab.Core.Render;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Render;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Core.Test {
    public sealed class GeometryComponent : Component {
        public HelixToolkit.Wpf.SharpDX.MeshGeometry3D Geometry { get; set; }

        public override string ToString() {
            return $"[Bounds:{Geometry.Bounds};Positions:{Geometry.Positions.Count};Indices:{Geometry.Indices.Count}]";
        }
    }
    public sealed class MaterialComponent : Component {
        public HelixToolkit.Wpf.SharpDX.PhongMaterial Material { get; set; }
        public HelixToolkit.Wpf.SharpDX.PhongMaterial BackMaterial { get; set; }
        public CullMode CullMaterial { get; set; }


        public void Setected() {
            var mat = new HelixToolkit.Wpf.SharpDX.PhongMaterial {
                AmbientColor = new Color4(),
                DiffuseColor = SharpDX.Color.Red,
                SpecularColor = SharpDX.Color.Red,
                EmissiveColor = new Color4(),
                ReflectiveColor = new Color4(),
                SpecularShininess = 100f
            };


            Material = mat;
            BackMaterial = mat;
        }
        public void UnSetected() {
            var mat = new HelixToolkit.Wpf.SharpDX.PhongMaterial {
                AmbientColor = new Color4(),
                DiffuseColor = SharpDX.Color.Blue,
                SpecularColor = SharpDX.Color.Blue,
                EmissiveColor = new Color4(),
                ReflectiveColor = new Color4(),
                SpecularShininess = 100f
            };


            Material = mat;
            BackMaterial = mat;
        }

        public override string ToString() {
            return $"MaterialComponent[{Material.DiffuseColor}]";
        }
    }
    public abstract class RenderTechniqueComponent : Component {
        public RenderTechnique RenderTechnique { get; set; }
    }
    public class PhongTechniqueRenderComponent : RenderTechniqueComponent {
        public PhongTechniqueRenderComponent() {
            RenderTechnique = Techniques.RenderPhong;
        }
        public override string ToString() {
            return $"[{RenderTechnique.Name}]";
        }
    }

    public sealed class TransformComponent : Component {
        public Matrix Matrix { get; set; }
    }
    
    public sealed class HitableComponent : Component {

    }
    public sealed class TargetedComponent : Component {

    }

    //builders

    public static class VisualModelBuilder {
        public static Entity Build(IEntityManager context, MeshGeometry3D geo, string tag) {
            //var geo = new MeshGeometry3D(new Vector3[] { Vector3.Zero, Vector3.Zero + Vector3.UnitX * 100, Vector3.Zero + Vector3.UnitY * 100 }, new int[] { 0, 1, 2 }, null);

            var mat = new HelixToolkit.Wpf.SharpDX.PhongMaterial {
                AmbientColor = new Color4(),
                DiffuseColor = SharpDX.Color.Blue,
                SpecularColor = SharpDX.Color.Blue,
                EmissiveColor = new Color4(),
                ReflectiveColor = new Color4(),
                SpecularShininess = 100f
            };
            

            var entity = context.CreateEntity(tag);
            entity.AddComponent(new GeometryComponent() { Geometry = geo });
            entity.AddComponent(new MaterialComponent {
                Material = mat,
                BackMaterial = mat,
                CullMaterial = CullMode.Back
            });
            entity.AddComponent(new Test.PhongTechniqueRenderComponent ());
            entity.AddComponent(new Test.TransformComponent { Matrix = SharpDX.Matrix.Identity });
            entity.AddComponent(new HitableComponent());

            return entity;
        }
    }
    public static class LightBuilder {

        public sealed class LightTechniqueRenderComponent : PhongTechniqueRenderComponent {
            public void Update(Graphics graphics, World world, Color4 color) {
                var variables = graphics.Variables(this.RenderTechnique);

                world.LightCount++;

                variables.LightCount.Set(world.LightCount);
                /// --- update lighting variables               
                variables.LightDir.Set(-world.LookDirection);
                variables.LightColor.Set(new[] { color });
                variables.LightType.Set(new[] { 1 /* (int)Light3D.Type.Directional*/ });
                
            }
        }

        public sealed class LightRenderComponent : Component {
            public Color4 Color { get; set; }
            public override string ToString() {
                return $"[Color:{Color}]";
            }
        }

        public static Entity BuildDirectionalLight(IEntityManager context) {
            var entity = context.CreateEntity("DirectionalLight");

            entity.AddComponent(new LightTechniqueRenderComponent());
            entity.AddComponent(new LightRenderComponent {
                Color = Color.White
            });

            return entity;
        }
    }
    public static class CameraBuilder {
        public enum CameraTypes {
            Perspective,
            Orthographic,
        }
        public sealed class CameraComponent : Component {
            public Vector3 Position { get; set; }
            public Vector3 LookDirection { get; set; }
            public Vector3 UpDirection { get; set; }
            public float NearPlaneDistance { get; set; }
            public int FarPlaneDistance { get; set; }
            public float Width { get; set; }
            public CameraTypes CameraType = CameraTypes.Orthographic;

            public Matrix CreateViewMatrix() {
                if (false) {// this.CreateLeftHandSystem) {
                    return global::SharpDX.Matrix.LookAtLH(Position, Position + LookDirection, UpDirection);
                }
                return global::SharpDX.Matrix.LookAtRH(Position, Position + LookDirection, UpDirection);
            }

            public Matrix CreateProjectionMatrix(double aspectRatio) {
                if (false) {// this.CreateLeftHandSystem) {
                    return Matrix.OrthoLH((float)Width, (float)(Width / aspectRatio), (float)NearPlaneDistance, (float)FarPlaneDistance);
                }
                float halfWidth = (float)(Width * 0.5f);
                float halfHeight = (float)(Width / aspectRatio) * 0.5f;
                Matrix projection;
                OrthoOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, (float)NearPlaneDistance, (float)FarPlaneDistance, out projection);
                return projection;
            }
            public static void OrthoOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix result) {
                float zRange = -2.0f / (zfar - znear);

                result = Matrix.Identity;
                result.M11 = 2.0f / (right - left);
                result.M22 = 2.0f / (top - bottom);
                result.M33 = zRange;
                result.M41 = ((left + right) / (left - right));
                result.M42 = ((top + bottom) / (bottom - top));
                result.M43 = -znear * zRange;
            }

            public Matrix GetFullMatrix(double aspectRatio) {
                return Matrix.Add(CreateViewMatrix(), CreateProjectionMatrix(aspectRatio));
            }

            public override string ToString() {
                return $"[Position:{Position};LookDirection:{LookDirection};UpDirection:{UpDirection};Width:{Width}]";
            }
        }
        public sealed class CameraTechniqueRenderComponent : PhongTechniqueRenderComponent {

            public void Update(Graphics graphics, World world, CameraComponent camera) {
                var variables = graphics.Variables(this.RenderTechnique);
                var aspectRatio = (float)graphics.SharpDevice.Width / graphics.SharpDevice.Height;
                
                var projectionMatrix = camera.CreateProjectionMatrix(aspectRatio);
                var viewMatrix = camera.CreateViewMatrix();
                var viewport = Vector4.Zero;
                var frustum = Vector4.Zero;
                variables.EyePos.Set(camera.Position);
                variables.Projection.SetMatrix(ref projectionMatrix);
                variables.View.SetMatrix(ref viewMatrix);
                //if (isProjCamera) {
                variables.Viewport.Set(ref viewport);
                variables.Frustum.Set(ref frustum);
                variables.EyeLook.Set(camera.LookDirection);


                world.Position = camera.Position;
                world.LookDirection = camera.LookDirection;
                world.ViewMatrix = viewMatrix;
                world.ProjectionMatrix = projectionMatrix;
            }
        }
        public static Entity BuildOrthographicCamera(IEntityManager context) {
            var entity = context.CreateEntity("OrthographicCamera");

            entity.AddComponent(new CameraComponent {
                Position = new Vector3(0, 0, 300),//50253
                LookDirection = new Vector3(0, 0, -300),
                UpDirection = new Vector3(0, 1, 0),
                NearPlaneDistance = 0,
                FarPlaneDistance = 100500,
                Width = 300
            });
            entity.AddComponent(new CameraTechniqueRenderComponent());

            return entity;
        }
    }
    public static class ViewportBuilder {
        public static Entity Build(IEntityManager context) {
            var view = context.CreateEntity("Viewport");

           // view.AddComponent();

            return view;
        }
    }


}
