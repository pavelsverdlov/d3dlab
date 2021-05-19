using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.FileFormats.GeoFormats;
using D3DLab.SDX.Engine.Components;
using D3DLab.Toolkit;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.D3Objects;
using D3DLab.Toolkit.Math3D;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace D3DLab.Viewer.D3D {
    struct LoadedObjectDetails {
        public int VertexCount { get; set; }
        public int TriangleCount { get; set; }
    }
    enum WorldAxisTypes {
        X,Y,Z,All
    }
    class LoadedVisualObject : MultiVisualObject {

        static readonly Vector4 color;
        static readonly Random random = new Random();

        VisualPolylineObject? bounds;

        VisualPolylineObject? worldX;
        VisualPolylineObject? worldY;
        VisualPolylineObject? worldZ;

        public LoadedObjectDetails Details { get; private set; }
        public SharpDX.Direct3D11.CullMode CullMode { get; private set; }

        static LoadedVisualObject() {
            var c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#B3B598");
            color = c.ToVector4();
        }
        public static LoadedVisualObject Create(IContextState context, IEnumerable<IFileGeometry3D> meshes,
            FileInfo texture, string name) {
            var visual = new LoadedVisualObject(name);
            visual.CullMode = SharpDX.Direct3D11.CullMode.Front;
            _Create(context, meshes, texture,name, visual);

            return visual;
        }
        static void _Create(IContextState context, IEnumerable<IFileGeometry3D> meshes,
            FileInfo texture, string name, LoadedVisualObject visual) {
            List<ElementTag> t = new List<ElementTag>();
            var details = new LoadedObjectDetails();
            var baseTag = ElementTag.New();
            var index = 0;
            AxisAlignedBox fullBox = AxisAlignedBox.Zero;
            foreach (var geo in meshes) {
                var cc = geo.Name.Split(',').Select(x => float.Parse(x.Trim(new[] { ' ', '<', '>' }), System.Globalization.NumberStyles.Float)).ToArray();
                var color = new Vector4(cc[0]/256f,cc[1] / 256f, cc[2] / 256f, 1);
                var tag = Create(context, baseTag.WithPrefix(geo.Name ?? index.ToString()),
                    new GeometryStructures<IFileGeometry3D>(geo), texture, out var box, color);
                t.Add(tag);
                fullBox = fullBox.Merge(box.Bounds);
                details.VertexCount += geo.Positions.Count;
                details.TriangleCount += (geo.Indices.Count / 3);
                index++;
            }

            visual.tags.AddRange(t);
            visual.Details = details;

            var size = fullBox.Size();

            visual.worldX = VisualPolylineObject.Create(context, baseTag.WithPrefix("WorldX"),
               new[] { Vector3.Zero + Vector3.UnitX * size.X * -2f, Vector3.Zero + Vector3.UnitX * size.X * 2f }, V4Colors.Red, false);
            visual.worldX.IsVisible = false;
            visual.worldY = VisualPolylineObject.Create(context, baseTag.WithPrefix("WorldY"),
               new[] { Vector3.Zero + Vector3.UnitY * size.Y * -2f, Vector3.Zero + Vector3.UnitY * size.Y * 2f }, V4Colors.Green, false);
            visual.worldY.IsVisible = false;
            visual.worldZ = VisualPolylineObject.Create(context, baseTag.WithPrefix("WorldZ"),
               new[] { Vector3.Zero + Vector3.UnitZ * size.Z * -2f, Vector3.Zero + Vector3.UnitZ * size.Z * 2f }, V4Colors.Blue, false);
            visual.worldZ.IsVisible = false;
        }

        static ElementTag Create(IContextState context, ElementTag tag, GeometryStructures gdata, FileInfo texture,
            out GeometryBoundsComponent boundsComponent, Vector4 color) {
            var manager = context.GetEntityManager();
            if (!gdata.Normals.Any()) {
                gdata.ReCalculateNormals();
            }
            
            gdata.BuildTreeAsync();

            var cullmode = SharpDX.Direct3D11.CullMode.Front;          
            var geo = context.GetGeometryPool().AddGeometry(gdata);

            var en = manager.CreateEntity(tag);

            MaterialColorComponent material;
            RenderableComponent renderable;
            if (gdata.TexCoor.Any() && texture != null) {
                material = MaterialColorComponent.CreateTransparent().ApplyAlpha(1);
                renderable = RenderableComponent.AsTriangleTexturedList(cullmode);
                en.AddComponent(new D3DTexturedMaterialSamplerComponent(texture));
            } else {
                material = MaterialColorComponent.Create(color);// V4Colors.NextColor(random));
                renderable = RenderableComponent.AsTriangleColoredList(cullmode);
            }
            boundsComponent = GeometryBoundsComponent.Create(gdata.Positions);

            en.AddComponent(TransformComponent.Identity())
              .AddComponent(HittableComponent.Create(0))
              .AddComponent(boundsComponent)
              .AddComponent(material)
              .AddComponent(geo)
              .AddComponent(renderable);

            return tag;
        }

        public void ReCreate(IContextState context, IEnumerable<IFileGeometry3D> meshes, FileInfo material, string name) {
            this.Cleanup(context);

            _Create(context, meshes, material, name, this);
        }

        LoadedVisualObject(string filename) : base(filename) {
        }

        //public override void Hide(IEntityManager manager) {
        //    foreach (var tag in Tags) {
        //        var en = manager.GetEntity(tag);
        //        var rend = en.GetComponent<RenderableComponent>();
        //        en.UpdateComponent(rend.Disable());
        //    }
        //}
        //public override void Show(IEntityManager manager) {
        //    foreach (var tag in Tags) {
        //        var en = manager.GetEntity(tag);
        //        var rend = en.GetComponent<RenderableComponent>();
        //        en.UpdateComponent(rend.Enable());
        //    }
        //}
        public override void Cleanup(IContextState context) {
            var manager = context.GetEntityManager();
            foreach (var tag in Tags) {
                manager.RemoveEntity(tag);
            }
          
            bounds?.Cleanup(context);
            worldX?.Cleanup(context);
            worldY?.Cleanup(context);
            worldZ?.Cleanup(context);

            base.Cleanup(context);
        }
        public GeometryStructures<IFileGeometry3D> GetMesh(IContextState context, in ElementTag tag) {
            var id = context.GetComponentManager().GetComponent<GeometryPoolComponent>(tag);
            return (GeometryStructures<IFileGeometry3D>)context.GetGeometryPool().GetGeometry<GeometryStructures>(id);
        }
        public T GetComponent<T>(IEntityManager manager, in ElementTag tag) where T : IGraphicComponent {
            return manager.GetEntity(tag).GetComponent<T>();
        }
        public void Transform(IEntityManager manager, in Matrix4x4 move) {
            foreach (var t in Tags) {
                manager.GetEntity(t).UpdateComponent(MovingComponent.Create(move));
            }
            worldX?.GetEntity(manager).UpdateComponent(MovingComponent.Create(move));
            worldY?.GetEntity(manager).UpdateComponent(MovingComponent.Create(move));
            worldZ?.GetEntity(manager).UpdateComponent(MovingComponent.Create(move));
            bounds?.GetEntity(manager).UpdateComponent(MovingComponent.Create(move));
        }
        public void ShowBoundingBox(IContextState context, out AxisAlignedBox fullBox) {
            if (bounds != null) throw new Exception("Bounds has already showed.");
            fullBox = GetAllBounds(context);
            bounds = VisualPolylineObject.CreateBox(context, ElementTag.New("Bounds_"), fullBox, V4Colors.White);
        }

        public AxisAlignedBox GetAllBounds(IContextState context) {
            var fullBox = new AxisAlignedBox();
            var manager = context.GetEntityManager();
            foreach (var t in Tags) {
                var en = manager.GetEntity(t);
                var renderable = en.GetComponent<RenderableComponent>();
                if (renderable.IsRenderable) {
                    var box = en.GetComponent<GeometryBoundsComponent>();
                    var tr = en.GetComponent<TransformComponent>();
                    fullBox = fullBox.Merge(box.Bounds.Transform(tr.MatrixWorld));
                }
            }
            return fullBox;
        }

        public void HideBoundingBox(IContextState context) {
            bounds?.Cleanup(context);
            bounds = null;           
        }

        public void ShowWorldAxis(IContextState context, WorldAxisTypes axis) {
            var manager = context.GetEntityManager();
            switch (axis) {
                case WorldAxisTypes.X when !worldX.IsVisible:
                    worldX.Show(context);
                    break;
                case WorldAxisTypes.Y when !worldY.IsVisible:
                    worldY.Show(context);
                    break;
                case WorldAxisTypes.Z when !worldZ.IsVisible:
                    worldZ.Show(context);
                    break;
            }
        }

       
        public void HideWorldAxis(IContextState context, WorldAxisTypes axis) {
            var manager = context.GetEntityManager();
            switch (axis) {
                case WorldAxisTypes.All when worldX.IsVisible:
                    worldX.Hide(context);
                    worldY.Hide(context);
                    worldZ.Hide(context);
                    break;
                case WorldAxisTypes.X when worldX.IsVisible:
                    worldX.Hide(context);
                    break;
                case WorldAxisTypes.Y when worldY.IsVisible:
                    worldY.Hide(context);
                    break;
                case WorldAxisTypes.Z when worldZ.IsVisible:
                    worldZ.Hide(context);
                    break;
            }
        }

        public void TurnFlatshadingOff(IContextState context) {
            var manager = context.GetComponentManager();
            foreach (var t in Tags) {
                manager.AddComponent(t, FlatShadingGeometryComponent.Create());
            }
        }
        public void TurnFlatshadingOn(IContextState context) {
            var manager = context.GetComponentManager();
            foreach (var t in Tags) {
                manager.RemoveComponent<FlatShadingGeometryComponent>(t);
            }
        }
        public void TurnSolidWireframeOff(IContextState context) {
            var manager = context.GetComponentManager();
            foreach (var t in Tags) {
                manager.RemoveComponent<WireframeGeometryComponent>(t);
            }
        }
        public void TurnSolidWireframeOn(IContextState context) {
            var manager = context.GetComponentManager();
            foreach (var t in Tags) {
                manager.AddComponent(t, WireframeGeometryComponent.Create());
            }
        }
        public void TurnTransparentWireframeOff(IContextState context) {
            var manager = context.GetEntityManager();
            foreach (var t in Tags) {
                var en = manager.GetEntity(t);
                en.UpdateComponent(en
                    .GetComponent<RenderableComponent>()
                    .SwitchFillModeTo(SharpDX.Direct3D11.FillMode.Solid));
            }
        }
        public void TurnTransparentWireframeOn(IContextState context) {
            var manager = context.GetEntityManager();
            foreach (var t in Tags) {
                var en = manager.GetEntity(t);
                en.UpdateComponent(en
                    .GetComponent<RenderableComponent>()
                    .SwitchFillModeTo(SharpDX.Direct3D11.FillMode.Wireframe));
            }
        }

        public void ChangeCullMode(IContextState context, SharpDX.Direct3D11.CullMode mode) {
            var manager = context.GetEntityManager();
            foreach (var t in Tags) {
                var en = manager.GetEntity(t);
                var renderable = en.GetComponent<RenderableComponent>();
                if (renderable.IsRenderable) {
                    en.UpdateComponent(en
                    .GetComponent<RenderableComponent>()
                    .SwitchCullModeTo(mode));
                }
            }
            this.CullMode = mode;
        }

    }
}
