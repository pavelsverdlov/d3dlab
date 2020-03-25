using D3DLab.FileFormats.GeometryFormats._OBJ;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace D3DLab.ConsoleTests {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Start");

            // Encoding.UTF8.GetBytes(new ReadOnlySpan<char>().);

            var file = new System.IO.FileInfo("Resources/i-robot obj.obj");
            var sw = new Stopwatch();

            var reader1 = new ObjByteReader();
            reader1.textureDir = file.Directory;
            reader1.Read(file);

            var count = 10;
            var tests = 10;
            //using (var str = file.OpenRead()) {
            while (tests-- > 0) {
                long total = 0;
                var iterations = count;

                while (iterations-- > 0) {
                    sw.Restart();

                    //var reader = new ObjReader();
                    //reader.textureDir = file.Directory;
                    //reader.Read(file);

                    //var reader1 = new ObjByteReader();
                    //reader1.textureDir = file.Directory;
                    //reader1.Read(file);

                    sw.Stop();
                    var milsec = sw.ElapsedMilliseconds;
                    total += milsec;
                    // str.Position = 0;
                }
                Console.WriteLine($"Total: {total / count} ms");
            }
            //   }

            Console.WriteLine($"Finished");
            Console.ReadKey();
        }
    }
}
