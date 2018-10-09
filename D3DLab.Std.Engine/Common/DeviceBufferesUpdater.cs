using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.Utilities;

namespace D3DLab.Std.Engine.Common {
    public class DeviceBufferesUpdater {
        DeviceBuffer indexBuffer;
        DeviceBuffer worldBuffer;
        DeviceBuffer vertexBuffer;

        public DeviceBuffer World { get => worldBuffer; }
        public DeviceBuffer Index { get => indexBuffer; }
        public DeviceBuffer Vertex { get => vertexBuffer; }

        DisposeCollectorResourceFactory factory;
        CommandList cmd;

        public void Update(DisposeCollectorResourceFactory factory, CommandList commands) {
            this.factory = factory;
            this.cmd = commands;
        }

        public void UpdateWorld() {
            factory.CreateIfNullBuffer(ref worldBuffer, new BufferDescription(64, BufferUsage.UniformBuffer));
            cmd.UpdateBuffer(worldBuffer, 0, Matrix4x4.Identity);
        }

        public void UpdateVertex<T>(T[] vertices, uint sizeInBytes) where T : struct {
            factory.CreateIfNullBuffer(ref vertexBuffer, new BufferDescription((uint)(sizeInBytes * vertices.Length),
               BufferUsage.VertexBuffer));
            cmd.UpdateBuffer(vertexBuffer, 0, vertices);
        }

        public void UpdateIndex(ushort[] indices) {
            factory.CreateIfNullBuffer(ref indexBuffer, new BufferDescription(sizeof(ushort) * (uint)indices.Length,
                 BufferUsage.IndexBuffer));
            cmd.UpdateBuffer(indexBuffer, 0, indices);
        }
    }
}
