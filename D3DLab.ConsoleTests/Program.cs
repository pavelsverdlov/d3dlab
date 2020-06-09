using D3DLab.FileFormats.GeometryFormats._OBJ;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;

namespace D3DLab.ConsoleTests {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Start");
            var file = new FileInfo(
                "Resources/i-robot obj.obj"
                );
            var sw = new Stopwatch();

            var tests = 10;
            long total = 0;
            sw.Restart();
            using (var reader = new FileFormats.MemoryMappedFileReader(file)) {
                var span = reader.ReadSpan();
                sw.Stop();
                var readTime = sw.ElapsedMilliseconds;
                Console.WriteLine($"Read span: {readTime} ms");

                while (tests-- > 0) {
                    if (span.Length != 113569792) {
                        Console.WriteLine($"ERROR: Not all data was readed");
                        break;
                    }

                    var parser = new Utf8ByteOBJParser();

                    sw.Restart();

                    parser.Read(span);

                    sw.Stop();

                    if (parser.FullGeometry.Indices.Count != 4692648 || parser.FullGeometry.Positions.Count != 786586) {
                        Console.WriteLine($"ERROR: Not all data was readed");
                        break;
                    }

                    var milsec = sw.ElapsedMilliseconds;
                    total += milsec;
                }
            }
            Console.WriteLine($"Total: {total / 10} ms");
            // DEBUG
            //1558 ms = 100 MB file 
            //1487 ms
            //1542 ms + Normals
            //1281 - refactor face
            //

            Console.WriteLine($"Finished");
            Console.ReadKey();
        }

        public class ObjStringReader {
            public readonly GroupGeometry3D FullGeometry = new GroupGeometry3D();
            public FileInfo MaterialFilePath { get; }

            readonly List<Vector3> positions = new List<Vector3>();
            readonly Dictionary<int, int> map = new Dictionary<int, int>();

            Dictionary<string, GroupGeometry3D> meshes;
            public void Read(FileInfo file) {

                var group = "g";
                var vector = "v";
                var face = "f";
                //Memory<byte> buffer = new Memory<byte>();
                var groupname = "noname";
                PartGeometry3D current = FullGeometry.CreatePart(groupname);
                using (var reader = new StreamReader(File.OpenRead(file.FullName))) {
                    while (!reader.EndOfStream) {
                        var span = reader.ReadLine();
                        if (span.StartsWith(group)) {
                            var start = span.IndexOf(' ');
                            groupname = span.Substring(start, span.Length - 1).Trim();
                            current = FullGeometry.CreatePart(groupname);
                        } else if (span.StartsWith(vector)) {
                            var part = span.Substring(2, span.Length - 2).Trim();
                            var val = SplitFloat(part, ' ');
                            var v = new Vector3(val[0], val[1], val[2]);
                            current.AddPosition(ref v);
                        } else if (span.StartsWith(face)) {
                            var part = span.Substring(2, span.Length - 2).Trim();
                            var val = SplitInt(part, ' ');
                            //var p = positions [val[0]];
                            //var p1 = positions[val[1]];
                            //var p2 = positions[val[2]];
                            //current.Indices.Add(val[0] - 1);
                            //current.Indices.Add(val[1] - 1);
                            //current.Indices.Add(val[2] - 1);
                            val[0] -= 1;
                            val[1] -= 1;
                            val[2] -= 1;
                            //current.AddTriangle(val);
                        }
                    }
                }
            }
            private float[] SplitFloat(string span, char separator) {
                var val = new float[3];
                var index = 0;
                while (index < 3) {
                    var end = span.IndexOf(' ');
                    if (end == -1) {
                        end = span.Length;
                    }
                    var part = span.Substring(0, end).Trim();
                    val[index] = float.Parse(part, CultureInfo.InvariantCulture);
                    index++;
                    span = span.Substring(end, span.Length - end).Trim();
                }
                return val;
            }
            private int[] SplitInt(string span, char separator) {
                var val = new int[3];
                var index = 0;
                while (index < 3) {
                    var end = span.IndexOf(' ');
                    if (end == -1) {
                        end = span.Length;
                    }
                    var part = span.Substring(0, end).Trim();
                    var face = part.Split('/');
                    val[index] = int.Parse(face[0], CultureInfo.InvariantCulture);
                    index++;
                    span = span.Substring(end, span.Length - end).Trim();
                }
                return val;
            }
        }
    }
}
