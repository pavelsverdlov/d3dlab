using D3DLab.Plugin.Contracts.Parsers;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Wpf.Engine.App;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace D3DLab.Visualization {
    class BaseEntityBuilder : IParseResultVisiter {
        protected readonly IEntityManager manager;
        protected readonly List<IGraphicComponent> components;

        public BaseEntityBuilder(IEntityManager manager) {
            this.manager = manager;
            components = new List<IGraphicComponent>();
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
    class EntityReBuilder : BaseEntityBuilder {
        readonly ElementTag entityTag;

        public EntityReBuilder(ElementTag tag, IEntityManager manager) : base(manager) {
            this.entityTag = tag;
        }

        public void ReBuildGeometry(Stream stream, IFileParserPlugin parser) {
            parser.Parse(stream, this);
            var en = manager.GetEntity(entityTag);
            if (components.All(x => x.IsValid)) {
                en.RemoveComponents<IGeometryComponent>();
                en.AddComponents(components);
            }            
        }
    }
    class EntityBuilder : BaseEntityBuilder {
        GraphicEntity entity;

        public EntityBuilder(IEntityManager manager) : base(manager) {
        }
        public ElementTag Build(Stream stream, IFileParserPlugin parser) {
            var tag = new ElementTag("Obj_" + DateTime.Now.Ticks);
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

    }
}
