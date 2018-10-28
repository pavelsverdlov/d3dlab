using D3DLab.Std.Engine.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace D3DLab.Std.Engine.Core.Systems {
    /* 
     * EXAMPLE HOW IT SHOUL BE IN SHADER!!!
     * 
    struct Light {
        uint Type;
        float Intensity;
        float3 Position;    
        float4 Color;         
    };
    cbuffer Light : register(b1) {
        Light Lights[];
    }
    */
    [StructLayout(LayoutKind.Explicit, Size = 64)]//32 + /* Color vector4 16 */ 16 + ???
    //[StructLayout(LayoutKind.Sequential)]
    public struct LightStructLayout {
        public const int MaxCount = 3;
        public const int RegisterResourceSlot = 1;

        [FieldOffset(0)]
        public readonly uint Type;
        [FieldOffset(4)] // + uint 4
        public readonly float Intensity;
        [FieldOffset(8)] // + float 4
        public readonly Vector3 Position;
        [FieldOffset(20)] // + vector3 12
        public readonly Vector3 Direction;
        [FieldOffset(32)] // + vector3 12
        public readonly Vector4 Color;

        public LightStructLayout(LightTypes type, Vector3 pos, Vector3 dir, Vector4 color, float intensity) {
            Type = (uint)type;
            Intensity = intensity;
            Position = pos;
            Direction = dir;
            Color = color;
        }
    }

    public struct LightState {
        public float Intensity;
        public Vector3 Position;
        public Vector3 Direction;
        public LightTypes Type;
        public Vector4 Color;

        public LightStructLayout GetStructLayoutResource() {
            return new LightStructLayout(Type, Position, Direction, Color, Intensity);
        }
    }

    public class LightsSystem : IComponentSystem {
        public void Execute(SceneSnapshot snapshot) {
            IEntityManager emanager = snapshot.ContextState.GetEntityManager();
            var camera = snapshot.Camera;

            try {
                foreach (var entity in emanager.GetEntities()) {
                    if (!entity.Has<ILightComponent>()) {
                        continue;
                    }
                    var light = entity.GetComponent<ILightComponent>();
                    var color = entity.GetComponent<ColorComponent>();

                    snapshot.UpdateLight(light.Index, new LightState {
                        Intensity = light.Intensity,
                        Position = light.Position,
                        Direction = light.Direction,
                        Color = color.Color,
                        Type = light.Type
                    });

                }
            } catch (Exception ex) {
                ex.ToString();
                throw ex;
            }
        }
    }
}
