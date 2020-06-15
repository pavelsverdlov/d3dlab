using D3DLab.FileFormats.GeoFormats._OBJ;

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

                    var builder = new UnitedGroupsBulder(parser.GeometryCache);
                    var mesh = builder.Build();

                    if (mesh.Indices.Count != 4692648 || mesh.Positions.Count != 786586) {
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
            //1920 - add OBJVertex
            //1530 - refactor OBJVertex

            Console.WriteLine($"Finished");
            Console.ReadKey();
        }

    }
}
