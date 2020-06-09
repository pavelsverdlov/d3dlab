using D3DLab.FileFormats.GeometryFormats._OBJ;

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

            Assert.AreEqual(parser.FullGeometry.Indices.Count, 3);
            var array = parser.FullGeometry.Indices.ToArray();
            Assert.AreEqual(array[0], 0);
            Assert.AreEqual(array[1], 1);
            Assert.AreEqual(array[2], 2);
        }
        [Test]
        public void TRIANGLE_FAN_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("f 1 2 3 4".ToCharArray()).AsSpan());

            Assert.AreEqual(parser.FullGeometry.Indices.Count, 6);
            var array = parser.FullGeometry.Indices.ToArray();
            Assert.AreEqual(array[0], 0);
            Assert.AreEqual(array[1], 1);
            Assert.AreEqual(array[2], 2);
            Assert.AreEqual(array[3], 0);
            Assert.AreEqual(array[4], 2);
            Assert.AreEqual(array[5], 3);
        }

        [Test]
        public void FACE_TRIPLE_SLASH_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("f 1/2/2 2/5/5 3/6/6 4/3/3".ToCharArray()).AsSpan());

            Assert.AreEqual(parser.FullGeometry.Indices.Count, 6);
            var array = parser.FullGeometry.Indices.ToArray();
            Assert.AreEqual(array[0], 0);
            Assert.AreEqual(array[1], 1);
            Assert.AreEqual(array[2], 2);
            Assert.AreEqual(array[3], 0);
            Assert.AreEqual(array[4], 2);
            Assert.AreEqual(array[5], 3);
        }

        [Test]
        public void SIMPLE_VERTEX_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("v -0.911915 123456.851241 -0.000001".ToCharArray()).AsSpan());

            var array = parser.FullGeometry.Positions.ToArray();
            Assert.AreEqual(parser.FullGeometry.Positions.Count, 1);
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

            Assert.AreEqual(parser.FullGeometry.Positions.Count, 1);
            Assert.AreEqual(parser.FullGeometry.Colors.Count, 1);
            
            var array = parser.FullGeometry.Positions.ToArray();
            Assert.AreEqual(array[0].X, 0.1f);
            Assert.AreEqual(array[0].Y, 9.021f);
            Assert.AreEqual(array[0].Z, -5.465, 0.0001);
            array = parser.FullGeometry.Colors.ToArray();
            Assert.AreEqual(array[0].X, 126.1f);
            Assert.AreEqual(array[0].Y, 127.2f);
            Assert.AreEqual(array[0].Z, 128.3f);
        }

        [Test]
        public void TEXTURE_COORDINATE_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("vt 0.1234 0.7492 0.0000"
                    .ToCharArray())
                .AsSpan());

            Assert.AreEqual(parser.FullGeometry.TextureCoors.Count, 1);
            var array = parser.FullGeometry.TextureCoors.ToArray();
            Assert.AreEqual(array[0].X, 0.1234f);
            Assert.AreEqual(array[0].Y, 1 - 0.7492f, 0.0001);
        }

        [Test]
        public void NORMAL_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("vn 0.0771 -0.5505 0.8313"
                    .ToCharArray())
                .AsSpan());

            Assert.AreEqual(parser.FullGeometry.Normals.Count, 1);
            var array = parser.FullGeometry.Normals.ToArray();
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

            Assert.AreEqual(parser.FullGeometry.Normals.Count, 0);
        }

        [Test]
        public void LF_CRLF_TEST() {
            var utf8 = Encoding.UTF8;
            var parser = new Utf8ByteOBJParser();
            parser.Read(utf8.GetBytes("v 0.1000 9.021 -5.465 126.1 127.2 128.3\nvn 0.0771 -0.5505 0.8313\r\nvt 0.1234 0.7492 0.0000"
                    .ToCharArray())
                .AsSpan());

            Assert.AreEqual(parser.FullGeometry.Positions.Count, 1);
            Assert.AreEqual(parser.FullGeometry.Normals.Count, 1);
            Assert.AreEqual(parser.FullGeometry.TextureCoors.Count, 1);
            Assert.AreEqual(parser.FullGeometry.Colors.Count, 1);
           
            var pos = parser.FullGeometry.Positions.ToArray();
            Assert.AreEqual(pos[0].X, 0.1f);
            Assert.AreEqual(pos[0].Y, 9.021f);
            Assert.AreEqual(pos[0].Z, -5.465, 0.0001);
            var colors = parser.FullGeometry.Colors.ToArray();
            Assert.AreEqual(colors[0].X, 126.1f);
            Assert.AreEqual(colors[0].Y, 127.2f);
            Assert.AreEqual(colors[0].Z, 128.3f);
            var tex = parser.FullGeometry.TextureCoors.ToArray();
            Assert.AreEqual(tex[0].X, 0.1234f);
            Assert.AreEqual(tex[0].Y, 1 - 0.7492f, 0.0001);
            var morm = parser.FullGeometry.Normals.ToArray();
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

            Assert.AreEqual(parser.FullGeometry.Parts.Count, 4);
            //[0] default group, not interesting
            var array = parser.FullGeometry.Parts.ToArray();
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
