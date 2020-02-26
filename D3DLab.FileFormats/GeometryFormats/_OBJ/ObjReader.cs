using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.FileFormats.GeometryFormats._OBJ {
    /// <remarks>
    /// See the file format specifications at
    /// http://en.wikipedia.org/wiki/Obj
    /// http://en.wikipedia.org/wiki/Material_Template_Library
    /// http://www.martinreddy.net/gfx/3d/OBJ.spec
    /// http://www.eg-models.de/formats/Format_Obj.html
    /// </remarks>
    public class ObjReader {
        public IFileGeometry3D FullGeometry => fullGeometry;

        readonly GroupGeometry3D fullGeometry = new GroupGeometry3D();

        public void Read(Stream stream) {
            var group = new ReadOnlySpan<char>(new[] { 'g' });
            var vector = new ReadOnlySpan<char>(new[] { 'v' });
            var texture = new ReadOnlySpan<char>(new[] { 'v', 't' });
            var face = new ReadOnlySpan<char>(new[] { 'f' });
            var comm = new ReadOnlySpan<char>(new[] { '#' });
            //Memory<byte> buffer = new Memory<byte>();
            var groupname = "noname";
            PartGeometry3D current = fullGeometry.CreatePart(groupname);
            using (var reader = new StreamReader(stream)) {
                while (!reader.EndOfStream) {
                    var span = reader.ReadLine().AsSpan();
                    if (span.StartsWith(comm) || span.IsWhiteSpace()) {
                        continue;
                    }
                    var part = span.Slice(2, span.Length - 2).Trim();
                    if (span.StartsWith(group)) {
                        var names = part.Trim().ToString().SplitOnWhitespace();
                        groupname = string.Join(" ", names);//[0].ToString();
                        var key = string.Join(" ", names.Take(names.Length - 1));//[0].ToString();
                        current = fullGeometry.CreatePart(groupname);
                    } else if (span.StartsWith(texture)) {
                        //vt u v w
                        // u is the value for the horizontal direction of the texture.
                        // v is an optional argument.
                        // v is the value for the vertical direction of the texture.The default is 0.
                        // w is an optional argument.
                        // w is a value for the depth of the texture.The default is 0.

                        var val = SplitFloat(part, ' ', 2);
                        var v = new Vector2(val[0], 1 - val[1]);
                        current.AddTextureCoor(ref v);
                    } else if (span.StartsWith(vector)) {
                        try {
                            var val = SplitFloat(part, ' ');
                            var v = new Vector3(val[0], val[1], val[2]);
                            current.AddPosition(ref v);
                        } catch (Exception ex) {
                            ex.ToString();
                        }

                    } else if (span.StartsWith(face)) {
                        var val = SplitInt(part, ' ');
                        if (new HashSet<int>(val).Count != 3) {

                        }
                        //FullGeometry1.Indices.AddRange(val);
                        try {
                            current.AddTriangle(val);
                        } catch (Exception ex) {
                            //TODO collect info for displaing in output 
                        }
                    }
                }
            }
        }
        private float[] SplitFloat(ReadOnlySpan<char> span, char separator, int count = 3) {
            var val = new float[3];
            var index = 0;
            while (index < count) {
                var end = span.IndexOf(' ');
                if (end == -1) {
                    end = span.Length;
                }
                var part = span.Slice(0, end).Trim();
                val[index] = float.Parse(part.ToString(), CultureInfo.InvariantCulture);
                index++;
                span = span.Slice(end, span.Length - end).Trim();
            }
            return val;
        }
        private List<int> SplitInt(ReadOnlySpan<char> span, char separator) {
            var val = new List<int>();
            var index = 0;
            while (!span.IsWhiteSpace()) {
                var end = span.IndexOf(' ');
                if (end == -1) {
                    end = span.Length;
                }
                var part = span.Slice(0, end).Trim();
                var sep = part.IndexOf('/');
                if (sep != -1) {
                    part = span.Slice(0, sep).Trim();
                }
                val.Add(int.Parse(part.ToString(), CultureInfo.InvariantCulture) - 1);
                index++;
                span = span.Slice(end, span.Length - end).Trim();
            }
            if (val.Count > 3) {
                var triangleFan = new List<int>();
                for (int i = 0; i + 2 < val.Count; i++) {
                    triangleFan.Add(val[0]);
                    triangleFan.Add(val[i + 1]);
                    triangleFan.Add(val[i + 2]);
                }
                return triangleFan;
            }

            return val;
        }
    }
}
