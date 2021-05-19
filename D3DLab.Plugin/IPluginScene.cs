using System.Numerics;

using D3DLab.ECS;

namespace D3DLab.Plugin {
    public struct ArrowDetails {
        public Vector3 Axis;
        public Vector3 Orthogonal;
        public Vector3 Center;
        public Vector4 Color;
    }
    public struct CylinderDetails {
        public Vector3 Axis;
        public Vector3 Start;
        public Vector4 Color;
        public float Length;
        public float Radius;
    }

    public interface IPluginScene {
        public IContextState Context { get; }

        GameObject DrawPoint(string key, Vector3 center, Vector4 color);
        GameObject DrawArrow(string key, ArrowDetails ad);
        GameObject DrawPolyline(string key, Vector3[] margin, Vector4 green);
        GameObject DrawCylinder(string key, CylinderDetails cyl);
    }
}