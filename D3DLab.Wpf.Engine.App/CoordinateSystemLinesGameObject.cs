using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Common;
using System.Numerics;

namespace D3DLab.Wpf.Engine.App {
    public class CoordinateSystemLinesGameObject {
        public ElementTag Lines { get; private set; }
        public ElementTag[] Arrows { get; private set; }

        public static CoordinateSystemLinesGameObject Build(IEntityManager manager) {
            var llength = 100;
            var obj = new CoordinateSystemLinesGameObject();
            var points = new[] {
                Vector3.Zero - Vector3.UnitX * llength, Vector3.Zero + Vector3.UnitX * llength,
                Vector3.Zero- Vector3.UnitY  * llength, Vector3.Zero + Vector3.UnitY * llength,
                Vector3.Zero- Vector3.UnitZ  * llength, Vector3.Zero + Vector3.UnitZ * llength,
            };
            var color = new[] {
                V4Colors.Green, V4Colors.Green,
                V4Colors.Red, V4Colors.Red,
                V4Colors.Blue, V4Colors.Blue,
            };
            obj.Lines = manager.BuildCoordinateSystemLinesEntity(new ElementTag("Coordinate System"),
                points, color);
            var lenght = 20.0f;
            var radius = 5.0f;
            
            obj.Arrows = new ElementTag[3];
            obj.Arrows[0] = manager.BuildArrow(new ArrowData {
                axis = Vector3.UnitZ,
                orthogonal = Vector3.UnitX,
                center = Vector3.Zero + Vector3.UnitZ * (llength - lenght),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Blue,
                tag = new ElementTag("Arrow_Z")
            });
            obj.Arrows[1] = manager.BuildArrow(new ArrowData {
                axis = Vector3.UnitX,
                orthogonal = Vector3.UnitY,
                center = Vector3.Zero + Vector3.UnitX * (llength - lenght),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Green,
                tag = new ElementTag("Arrow_X")
            });
            obj.Arrows[2] = manager.BuildArrow(new ArrowData {
                axis = Vector3.UnitY,
                orthogonal = Vector3.UnitZ,
                center = Vector3.Zero + Vector3.UnitY * (llength - lenght),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Red,
                tag = new ElementTag("Arrow_Y")
            });

            return obj;
        }
        public CoordinateSystemLinesGameObject() {

        }
    }
}
