using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Plugin.Import.OBJ {
    class ObjReader {
        public readonly AbstractGeometry3D FullGeometry1 = new AbstractGeometry3D();
        public readonly GroupGeometry3D FullGeometry = new GroupGeometry3D();

        Dictionary<string, PartGeometry3D> meshes;

        public void Read(Stream stream) {
            meshes = new Dictionary<string, PartGeometry3D>();

            var group = new ReadOnlySpan<char>(new[] { 'g' });
            var vector = new ReadOnlySpan<char>(new[] { 'v' });
            var texture = new ReadOnlySpan<char>(new[] { 'v', 't' });
            var face = new ReadOnlySpan<char>(new[] { 'f' });
            var comm = new ReadOnlySpan<char>(new[] { '#' });
            //Memory<byte> buffer = new Memory<byte>();
            var groupname = "noname";
            PartGeometry3D current = FullGeometry.CreatePart(groupname);
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
                        current = FullGeometry.CreatePart(groupname);
                    } else if (span.StartsWith(texture)) {

                    } else if (span.StartsWith(vector)) {
                        try {
                            var val = SplitFloat(part, ' ');
                            var v = new Vector3(val[0], val[1], val[2]);

                            FullGeometry1.Positions.Add(v);
                            current.AddPosition(ref v);
                        } catch (Exception ex) {
                            ex.ToString();
                        }

                    } else if (span.StartsWith(face)) {
                        var val = SplitInt(part, ' ');

                        if (new HashSet<int>(val).Count != 3) {

                        }

                        FullGeometry1.Indices.AddRange(val);
                        try {
                            current.AddTriangle(val);
                        } catch (Exception ex) {
                            //TODO collect info for displaing in output 
                        }
                    }
                }
            }
        }
        private float[] SplitFloat(ReadOnlySpan<char> span, char separator) {
            var val = new float[3];
            var index = 0;
            while (index < 3) {
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
