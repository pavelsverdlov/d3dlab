using D3DLab.FileFormats.GeoFormats.STL;

using NUnit.Framework;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace D3DLab.NUnitTests {
    class FileBinarySTLFormatTest {
        [Test]
        public void TRIANGLE_TEST() {
            var utf8 = Encoding.UTF8;
            var f = new FileInfo(@"...");
            var parser = new ASCIIBinarySTLParser();
            //using (var reader = new FileFormats.MemoryMappedFileReader(f)) {
            //    parser.Read(reader.ReadSpan());
            //}
            using (var str = File.OpenRead(f.FullName)) {
                parser.Read(str);
            }
        }



    }
}
