using D3DLab.Plugin.Contracts.Parsers;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Wpf.Engine.App;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

namespace D3DLab.Visualization {
    class EntityBuilder : IParseResultVisiter {
        readonly IEntityManager manager;
        GraphicEntity entity;

        readonly List<IGraphicComponent> components;
        public EntityBuilder(IEntityManager manager) {
            this.manager = manager;
            components = new List<IGraphicComponent>();
        }
        public ElementTag Build(Stream stream, IFileParserPlugin parser) {
            var tag = new ElementTag("Obj_"+DateTime.Now.Ticks);
            entity = manager.CreateEntity(tag);

            parser.Parse(stream, this);

            components.Add(EntityBuilders.GetRenderAsTriangleColored());
            components.Add(EntityBuilders.GetTransformation());

            entity.AddComponents(components);

            return tag;
        }
        public ElementTag Build(FileInfo file, IFileParserPlugin parser) {
            using (var str = File.OpenRead(file.FullName)) {
                Build(str, parser);
            }
            return entity.Tag;
        }
        public void Handle(AbstractGeometry3D mesh) {
            var com = new Std.Engine.Core.Components.GeometryComponent {
                Positions = mesh.Positions.ToImmutableArray(),
                Indices = mesh.Indices.ToImmutableArray(),
                Normals = mesh.Normals.ToImmutableArray(),
                Color = mesh.Color
            };
            components.Add(com);
        }

        public void Handle(IEnumerable<AbstractGeometry3D> mesh) {
            var group = new Std.Engine.Core.Components.GroupGeometryComponent();
            mesh.ForEach(x => group.Add(new Std.Engine.Core.Components.GeometryComponent {
                Positions = x.Positions.ToImmutableArray(),
                Indices = x.Indices.ToImmutableArray(),
                Normals = x.Normals.ToImmutableArray(),
                Color = x.Color
            }));
            group.Combine();
            components.Add(group);
        }
    }
}
