using D3DLab.ECS;
using D3DLab.FileFormats.GeoFormats._OBJ;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace D3DLab.NUnitTests {
    class FileFormatsTest {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public void TRIANGLE_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("f 1 2 3".ToCharArray()).AsSpan());

            var mesh = parser.GeometryCache.Groups.First();

            Assert.AreEqual(mesh.Vertices.Count, 3);
            var array = mesh.Vertices.ToArray();
            Assert.AreEqual(array[0].V, 0);
            Assert.AreEqual(array[1].V, 1);
            Assert.AreEqual(array[2].V, 2);
        }
        [Test]
        public void TRIANGLE_FAN_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("f 1 2 3 4".ToCharArray()).AsSpan());

            var mesh = parser.GeometryCache.Groups.First();

            Assert.AreEqual(mesh.Vertices.Count, 6);
            var array = mesh.Vertices.ToArray();
            Assert.AreEqual(array[0].V, 0);
            Assert.AreEqual(array[1].V, 1);
            Assert.AreEqual(array[2].V, 2);
            Assert.AreEqual(array[3].V, 0);
            Assert.AreEqual(array[4].V, 2);
            Assert.AreEqual(array[5].V, 3);
        }

        [Test]
        public void FACE_TRIPLE_SLASH_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("f 1/2/2 2/5/5 3/6/6 4/3/3".ToCharArray()).AsSpan());

            var v = parser.GeometryCache.Groups.First().Vertices;

            Assert.AreEqual(v.Count, 6);
            var array = v.ToArray();
            Assert.AreEqual(array[0].V, 0);
            Assert.AreEqual(array[1].V, 1);
            Assert.AreEqual(array[2].V, 2);
            Assert.AreEqual(array[3].V, 0);
            Assert.AreEqual(array[4].V, 2);
            Assert.AreEqual(array[5].V, 3);
        }
        
        [Test]
        public void FACE_TRIPLE_NO_TEXTURE_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();//
            parser.Read(utf8.GetBytes("f 1//1 2//2 3//3 4//4".ToCharArray()).AsSpan());
            //v/vt/vn
            var v = parser.GeometryCache.Groups.First().Vertices;

            Assert.AreEqual(v.Count, 6);
            var array = v.ToArray();
            Assert.AreEqual(array[0].V, 0);
            Assert.AreEqual(array[1].V, 1);
            Assert.AreEqual(array[2].V, 2);
            Assert.AreEqual(array[3].V, 0);
            Assert.AreEqual(array[4].V, 2);
            Assert.AreEqual(array[5].V, 3);

            Assert.AreEqual(array[0].VN, 0);
            Assert.AreEqual(array[1].VN, 1);
            Assert.AreEqual(array[2].VN, 2);
            Assert.AreEqual(array[3].VN, 0);
            Assert.AreEqual(array[4].VN, 2);
            Assert.AreEqual(array[5].VN, 3);

            Assert.AreEqual(array[0].VT, -2);
            Assert.AreEqual(array[1].VT, -2);
            Assert.AreEqual(array[2].VT, -2);
            Assert.AreEqual(array[3].VT, -2);
            Assert.AreEqual(array[4].VT, -2);
            Assert.AreEqual(array[5].VT, -2);
        }
        [Test]
        public void FACE_TRIPLE_NO_NORMAL_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();//
            parser.Read(utf8.GetBytes("f 1/1 2/2 3/3 4/4".ToCharArray()).AsSpan());
            //v/vt/vn
            var v = parser.GeometryCache.Groups.First().Vertices;

            Assert.AreEqual(v.Count, 6);
            var array = v.ToArray();
            Assert.AreEqual(array[0].V, 0);
            Assert.AreEqual(array[1].V, 1);
            Assert.AreEqual(array[2].V, 2);
            Assert.AreEqual(array[3].V, 0);
            Assert.AreEqual(array[4].V, 2);
            Assert.AreEqual(array[5].V, 3);

            Assert.AreEqual(array[0].VT, 0);
            Assert.AreEqual(array[1].VT, 1);
            Assert.AreEqual(array[2].VT, 2);
            Assert.AreEqual(array[3].VT, 0);
            Assert.AreEqual(array[4].VT, 2);
            Assert.AreEqual(array[5].VT, 3);

            Assert.AreEqual(array[0].VN, -2);
            Assert.AreEqual(array[1].VN, -2);
            Assert.AreEqual(array[2].VN, -2);
            Assert.AreEqual(array[3].VN, -2);
            Assert.AreEqual(array[4].VN, -2);
            Assert.AreEqual(array[5].VN, -2);
        }

        [Test]
        public void SIMPLE_VERTEX_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("v -0.911915 123456.851241 -0.000001".ToCharArray()).AsSpan());

            var mesh = parser.GeometryCache;

            var array = mesh.PositionsCache;
            Assert.AreEqual(mesh.PositionsCache.Count, 1);
            Assert.AreEqual(array[0].X, -0.911915f);
            Assert.AreEqual(array[0].Y, 123456.851241f);
            Assert.AreEqual(array[0].Z, -0.000001f);
        }

        [Test]
        public void VERTEX_COLOR_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("v 0.1000 9.021 -5.465 126.1 127.2 128.3"
                    .ToCharArray())
                .AsSpan());

            var mesh = parser.GeometryCache;

            Assert.AreEqual(mesh.PositionsCache.Count, 1);
            Assert.AreEqual(mesh.ColorsCache.Count, 1);

            var array = mesh.PositionsCache;
            Assert.AreEqual(array[0].X, 0.1f);
            Assert.AreEqual(array[0].Y, 9.021f);
            Assert.AreEqual(array[0].Z, -5.465, 0.0001);
            array = mesh.ColorsCache;
            Assert.AreEqual(array[0].X, 126.1f);
            Assert.AreEqual(array[0].Y, 127.2f);
            Assert.AreEqual(array[0].Z, 128.3f);
        }

        [Test]
        public void TEXTURE_COORDINATE_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("v 0 0 0\nvt 0.1234 0.7492 0.0000\nf 1 1 1"
                    .ToCharArray())
                .AsSpan());

            var mesh = parser.GeometryCache;

            Assert.AreEqual(mesh.TextureCoorsCache.Count, 1);
            var array = mesh.TextureCoorsCache;
            Assert.AreEqual(array[0].X, 0.1234f);
            Assert.AreEqual(array[0].Y, 1 - 0.7492f, 0.0001);
        }

        // NO_TEXTURE_COORDINATE_FOR_ONE_OF_GROUPS_TEST
        // NO_NORMAL_FOR_ONE_OF_GROUPS_TEST

        [Test]
        public void NORMAL_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("v 0 0 0\nvn 0.0771 -0.5505 0.8313\nf 1 1 1"
                    .ToCharArray())
                .AsSpan());

            var mesh = parser.GeometryCache;

            Assert.AreEqual(mesh.NormalsCache.Count, 1);
            var array = mesh.NormalsCache;
            Assert.AreEqual(array[0].X, 0.0771f);
            Assert.AreEqual(array[0].Y, -0.5505f);
            Assert.AreEqual(array[0].Z, 0.8313f);
        }

        [Test]
        public void COMMENTS_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("# vn 0.0771 -0.5505 0.8313"
                    .ToCharArray())
                .AsSpan());

            var mesh = parser.GeometryCache;

            Assert.AreEqual(mesh.NormalsCache.Count, 0);
        }

        [Test]
        public void LF_CRLF_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("v 0.1000 9.021 -5.465 126.1 127.2 128.3\nvn 0.0771 -0.5505 0.8313\r\nvt 0.1234 0.7492 0.0000\nf 1 1 1"
                    .ToCharArray())
                .AsSpan());

            var mesh = parser.GeometryCache;

            Assert.AreEqual(mesh.PositionsCache.Count, 1);
            Assert.AreEqual(mesh.NormalsCache.Count, 1);
            Assert.AreEqual(mesh.TextureCoorsCache.Count, 1);
            Assert.AreEqual(mesh.ColorsCache.Count, 1);
           
            var pos = mesh.PositionsCache.ToArray();
            Assert.AreEqual(pos[0].X, 0.1f);
            Assert.AreEqual(pos[0].Y, 9.021f);
            Assert.AreEqual(pos[0].Z, -5.465, 0.0001);
            var colors = mesh.ColorsCache.ToArray();
            Assert.AreEqual(colors[0].X, 126.1f);
            Assert.AreEqual(colors[0].Y, 127.2f);
            Assert.AreEqual(colors[0].Z, 128.3f);
            var tex = mesh.TextureCoorsCache.ToArray();
            Assert.AreEqual(tex[0].X, 0.1234f);
            Assert.AreEqual(tex[0].Y, 1 - 0.7492f, 0.0001);
            var morm = mesh.NormalsCache.ToArray();
            Assert.AreEqual(morm[0].X, 0.0771f);
            Assert.AreEqual(morm[0].Y, -0.5505f);
            Assert.AreEqual(morm[0].Z, 0.8313f);
        }

        [Test]
        public void GROUPS_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes(@"g A    
g A    B
g AB     C"
                    .ToCharArray())
                .AsSpan());

            Assert.AreEqual(parser.GeometryCache.Groups.Count, 4);
            //[0] default group, not interesting
            var array = parser.GeometryCache.Groups.ToArray();
            Assert.AreEqual(array[1].Name, "A");
            Assert.AreEqual(array[2].Name, "A B");
            Assert.AreEqual(array[3].Name, "AB C");
        }

        [Test]
        public void MATERIAL_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes(@"mtllib i-robot obj.mtl"
                    .ToCharArray())
                .AsSpan());
            Assert.AreEqual(parser.MtlFileName, "obj.mtl");
        }

        [Test]
        public void PARSE_MTL_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            var mtl = @"newmtl material_0
map_Kd MaterialImageFile.jpg
";
            string image = null;
            using (var memory = new MemoryStream()) {
                memory.Write(utf8.GetBytes(mtl));
                memory.Position = 0;
                using (var reader = new StreamReader(memory)) {
                    image = parser.GetMaterialFilePath(new DirectoryInfo("J://images/"), reader);
                }
            }
            Assert.AreEqual(image, @"J:\images\MaterialImageFile.jpg");
        }
    }
}
