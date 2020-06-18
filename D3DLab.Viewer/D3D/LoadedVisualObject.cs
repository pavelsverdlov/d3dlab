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
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace D3DLab.Viewer.D3D {
    class LoadedVisualObject : GeometryGameObject {

        static readonly Vector4 color;
        static readonly Random random = new Random();
        public List<ElementTag> Tags { get; }
        public string FileName { get; }

        static LoadedVisualObject() {
            var c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#B3B598");
            color = c.ToVector4();
        }
        public static LoadedVisualObject Create(IContextState context, IEnumerable<IFileGeometry3D> obj,
            FileInfo texture, string name) {
            List<ElementTag> t = new List<ElementTag>();
            foreach (var geo in obj) {
                var tag = Create(context, new GeometryStructures<IFileGeometry3D>(geo), texture);
                t.Add(tag);
            }
            return new LoadedVisualObject(t, name);
        }

        static ElementTag Create(IContextState context, GeometryStructures gdata, FileInfo texture) {
            var manager = context.GetEntityManager();
            if (!gdata.Normals.Any()) {
                gdata.ReCalculateNormals();
            }

            gdata.BuildTreeAsync();

            var cullmode = SharpDX.Direct3D11.CullMode.Front;
            var tag = ElementTag.New();
            var geo = context.GetGeometryPool().AddGeometry(gdata);

            var en = manager.CreateEntity(tag);

            MaterialColorComponent material;
            RenderableComponent renderable;
            if (gdata.TexCoor.Any() && texture != null) {
                material = MaterialColorComponent.CreateTransparent().ApplyAlpha(1);
                renderable = RenderableComponent.AsTriangleTexturedList(cullmode);
                en.AddComponent(new D3DTexturedMaterialSamplerComponent(texture));
            } else {
                material = MaterialColorComponent.Create(V4Colors.NextColor(random));
                renderable = RenderableComponent.AsTriangleColoredList(cullmode);
            }

            en.AddComponents(
                    TransformComponent.Identity(),
                    HittableComponent.Create(0),
                    GeometryBoundsComponent.Create(gdata.Positions),
                    material,
                    geo,
                    renderable
                ); ;

            return tag;
        }

        public override void Hide(IEntityManager manager) {
            foreach (var tag in Tags) {
                var en = manager.GetEntity(tag);
                var rend = en.GetComponent<RenderableComponent>();
                en.UpdateComponent(rend.Disable());
            }
        }
        public override void Show(IEntityManager manager) {
            foreach (var tag in Tags) {
                var en = manager.GetEntity(tag);
                var rend = en.GetComponent<RenderableComponent>();
                en.UpdateComponent(rend.Enable());
            }
        }
        public override void Cleanup(IEntityManager manager) {
            foreach(var tag in Tags) {
                manager.RemoveEntity(tag);
            }
            base.Cleanup(manager);
        }
        public GeometryStructures<IFileGeometry3D> GetMesh(IContextState context, in ElementTag tag) {
            var id = context.GetComponentManager().GetComponent<GeometryPoolComponent>(tag);
            return (GeometryStructures<IFileGeometry3D>)context.GetGeometryPool().GetGeometry<GeometryStructures>(id);
        }
        public T GetComponent<T>(IEntityManager manager,in ElementTag tag) where T : IGraphicComponent {
            return manager.GetEntity(tag).GetComponent<T>();
        }
        public void UpdateComponent<T>(IEntityManager manager, T com) where T : IGraphicComponent {
            foreach (var t in Tags) {
                manager.GetEntity(t).UpdateComponent(com);
            }
        }

        LoadedVisualObject(List<ElementTag> tag, string filename) : base(filename) {
            this.Tags = tag;
        }
    }
}
