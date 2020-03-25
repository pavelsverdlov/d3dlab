using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        static readonly char[] winnewlineChars;
        static readonly char[] unixnewlineChars;
        static readonly char[] groupChars;
        static readonly char[] vectorChars;
        static readonly char[] textureChars;
        static readonly char[] faceChars;
        static readonly char[] commChars;
        static readonly char[] materialChars;
        static readonly char[] newmtlChars;
        static readonly char[] diffuseMapChars;

        const char spaceChar = ' ';

        static ObjReader() {
            winnewlineChars = new[] { '\r', '\n' };
            unixnewlineChars = new[] { '\n' };
            groupChars = new[] { 'g' };
            vectorChars = new[] { 'v' };
            textureChars = new[] { 'v', 't' };
            faceChars = new[] { 'f' };
            commChars = new[] { '#' };
            materialChars = new[] { 'm', 't', 'l', 'l', 'i', 'b' };
            newmtlChars = new[] { 'n', 'e', 'w', 'm', 't', 'l' };
            diffuseMapChars = new[] { 'm', 'a', 'p', '_', 'K', 'd' };
        }
        public IFileGeometry3D FullGeometry => fullGeometry;

        readonly GroupGeometry3D fullGeometry = new GroupGeometry3D();
        public FileInfo MaterialFilePath { get; private set; }

        public DirectoryInfo textureDir;
        public void Read(FileInfo file) {
            textureDir = file.Directory;

            ReadOnlySpan<char> all;
            using (var str = file.OpenRead()) {
                all = new StreamReader(str).ReadToEnd().AsSpan();
            }
            Read(ref all);
        }
        unsafe void Read(ref ReadOnlySpan<char> all) {

            #region consts

            var winnewline = new ReadOnlySpan<char>(winnewlineChars);
            var unixnewline = new ReadOnlySpan<char>(unixnewlineChars);
            var group = new ReadOnlySpan<char>(groupChars);
            var vector = new ReadOnlySpan<char>(vectorChars);
            var texture = new ReadOnlySpan<char>(textureChars);
            var face = new ReadOnlySpan<char>(faceChars);
            var comm = new ReadOnlySpan<char>(commChars);
            var material = new ReadOnlySpan<char>(materialChars);

            #endregion


            var groupname = "noname";
            var current = fullGeometry.CreatePart(groupname);

            //var all = new StreamReader(stream).ReadToEnd().AsSpan();
            var floats = new float[3];


            while (!all.IsEmpty) {
                var endLine = all.IndexOf(winnewline);
                int separatorLenght = winnewline.Length;
                if (endLine == -1) {
                    endLine = all.IndexOf(unixnewline);
                    separatorLenght = unixnewline.Length;
                }
                if (endLine == -1) {
                    endLine = all.Length;
                    separatorLenght = 0;
                }
                var line = all.Slice(0, endLine);
                all = all.Slice(endLine + separatorLenght);

                if (line.StartsWith(comm) || line.IsWhiteSpace()) {
                    continue;
                }

                var part = line.Slice(2, line.Length - 2).Trim();
                if (line.StartsWith(group)) {
                    var names = part.Trim().ToString().SplitOnWhitespace();
                    groupname = string.Join(" ", names);//[0].ToString();
                    var key = string.Join(" ", names.Take(names.Length - 1));//[0].ToString();
                    current = fullGeometry.CreatePart(groupname);
                } else if (line.StartsWith(texture)) {
                    //vt u v w
                    // u is the value for the horizontal direction of the texture.
                    // v is an optional argument.
                    // v is the value for the vertical direction of the texture.The default is 0.
                    // w is an optional argument.
                    // w is a value for the depth of the texture.The default is 0.

                    SplitVertex(ref part, floats, 2);
                    var v = new Vector2(floats[0], 1 - floats[1]);
                    current.AddTextureCoor(ref v);
                } else if (line.StartsWith(vector)) {
                    try {
                        SplitVertex(ref part, floats);
                        var v = new Vector3(floats[0], floats[1], floats[2]);
                        current.AddPosition(ref v);
                    } catch (Exception ex) {
                        ex.ToString();
                    }

                } else if (line.StartsWith(face)) {
                    var val = SplitFace(ref part);
                    if (new HashSet<int>(val).Count != 3) {

                    }
                    //FullGeometry1.Indices.AddRange(val);
                    try {
                        current.AddTriangle(val);
                    } catch (Exception ex) {
                        //TODO collect info for displaing in output 
                    }
                } else if (line.StartsWith(material)) {
                    //mtllib filename.mat
                    var end = line.IndexOf(spaceChar);
                    var filename = line.Slice(end + 1, line.Length - end - 1);
                    LoadMaterial(ref filename);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static void SplitVertex(ref ReadOnlySpan<char> span, float[] val, int count = 3) {
            var index = 0;
            while (index < count) {
                var end = span.IndexOf(spaceChar);
                if (end == -1) {
                    end = span.Length;
                }
                var part = span.Slice(0, end).Trim();

                val[index] = float.Parse(part, NumberStyles.Float, CultureInfo.InvariantCulture);
                index++;
                span = span.Slice(end, span.Length - end).Trim();
            }
        }

        static List<int> SplitFace(ref ReadOnlySpan<char> span) {
            var val = new List<int>();
            var index = 0;
            while (!span.IsWhiteSpace()) {
                var end = span.IndexOf(spaceChar);
                if (end == -1) {
                    end = span.Length;
                }
                var part = span.Slice(0, end).Trim();
                var sep = part.IndexOf('/');
                if (sep != -1) {
                    part = span.Slice(0, sep).Trim();
                }

                val.Add(int.Parse(part, NumberStyles.Integer, CultureInfo.InvariantCulture) - 1);
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

        void LoadMaterial(ref ReadOnlySpan<char> part) {
            var splitBySpace = part.IndexOf(spaceChar);
            if (splitBySpace != -1) {
                part = part.Slice(splitBySpace, part.Length - splitBySpace);
            }
            var fileName = part.ToString();
            var fullPath = Path.Combine(textureDir.FullName, fileName);
            if (!File.Exists(fullPath)) {
                return;
            }
            var newmtl = new ReadOnlySpan<char>(newmtlChars);
            var diffuseMap = new ReadOnlySpan<char>(diffuseMapChars);//map_Kd

            using (var mreader = new StreamReader(fullPath)) {
                while (!mreader.EndOfStream) {
                    var span = mreader.ReadLine().AsSpan();
                    if (span.StartsWith(newmtl)) {
                        //ignore for now, need to create list of materials
                    } else if (span.StartsWith(diffuseMap)) {
                        var end = span.IndexOf(spaceChar);
                        var filename = span.Slice(end + 1, span.Length - end - 1);
                        MaterialFilePath = new FileInfo(Path.Combine(textureDir.FullName, filename.ToString()));

                        return; //support only one material for now
                    }
                }
            }
        }
    }

    public class ObjByteReader {
        static readonly byte[] winnewlineChars;
        static readonly byte[] unixnewlineChars;
        static readonly char[] groupChars;
        static readonly char[] vectorChars;
        static readonly char[] textureChars;
        static readonly char[] faceChars;
        static readonly char[] commChars;
        static readonly char[] materialChars;
        static readonly char[] newmtlChars;
        static readonly char[] diffuseMapChars;
        static readonly char[] normalChars;

        const char spaceChar = ' ';


        static ObjByteReader() {
            var utf8 = Encoding.UTF8;
            winnewlineChars = utf8.GetBytes(new[] { '\r', '\n' });
            unixnewlineChars = utf8.GetBytes(new[] { '\n' });
            groupChars = new[] { 'g' };
            vectorChars = new[] { 'v' };
            textureChars = new[] { 'v', 't' };
            normalChars = new[] { 'v', 'n' };
            faceChars = new[] { 'f' };
            commChars = new[] { '#' };
            materialChars = new[] { 'm', 't', 'l', 'l', 'i', 'b' };
            newmtlChars = new[] { 'n', 'e', 'w', 'm', 't', 'l' };
            diffuseMapChars = new[] { 'm', 'a', 'p', '_', 'K', 'd' };
        }
        public IFileGeometry3D FullGeometry => fullGeometry;

        readonly GroupGeometry3D fullGeometry = new GroupGeometry3D();
        public FileInfo MaterialFilePath { get; private set; }

        public DirectoryInfo textureDir;
        public void Read(FileInfo file) {
            textureDir = file.Directory;


            //   using (var str = file.OpenRead()) {
            //   using (var reader = new BinaryReader(str, Encoding.UTF8)) 

            // UnmanagedMemoryStream d = (UnmanagedMemoryStream)new StreamReader(str).;

            var mm = MemoryMappedFile.CreateFromFile(file.FullName, FileMode.Open);
            //var mm = MemoryMappedFile.CreateFromFile(str, null, 0, MemoryMappedFileAccess.Read,
            //HandleInheritability.Inheritable, true);
            //var va = mm.CreateViewAccessor();
            var va = mm.CreateViewStream();
            var mma = va.SafeMemoryMappedViewHandle;
            ReadOnlySpan<byte> bytes;
            unsafe {
                byte* ptrMemMap = (byte*)0;
                mma.AcquirePointer(ref ptrMemMap);
                bytes = new ReadOnlySpan<byte>(ptrMemMap, (int)mma.ByteLength);
            }

            //    var all = MemoryMarshal.AsBytes<char>(bytes);
            //  var all = MemoryMarshal.Cast<byte, char>(bytes);



            Read(ref bytes);
            //   }
            //   
            // }
        }
        public unsafe void Read(ref ReadOnlySpan<byte> all) {

            #region consts

            var winnewline = new ReadOnlySpan<byte>(winnewlineChars);
            var unixnewline = new ReadOnlySpan<byte>(unixnewlineChars);
            var group = new ReadOnlySpan<char>(groupChars);
            var vector = new ReadOnlySpan<char>(vectorChars);
            var texture = new ReadOnlySpan<char>(textureChars);
            var face = new ReadOnlySpan<char>(faceChars);
            var comm = new ReadOnlySpan<char>(commChars);
            var material = new ReadOnlySpan<char>(materialChars);
            var normal = new ReadOnlySpan<char>(normalChars);

            #endregion

            var utf8 = Encoding.UTF8;
            var groupname = "noname";
            var current = fullGeometry.CreatePart(groupname);

            //var all = new StreamReader(stream).ReadToEnd().AsSpan();
            var floats = new float[3];


            while (!all.IsEmpty) {
                var endLine = all.IndexOf(winnewline);
                int separatorLenght = winnewline.Length;
                if (endLine == -1) {
                    endLine = all.IndexOf(unixnewline);
                    separatorLenght = unixnewline.Length;
                }
                if (endLine == -1) {
                    endLine = all.Length;
                    separatorLenght = 0;
                }

                ReadOnlySpan<char> line;
                
                fixed (byte* buffer = &MemoryMarshal.GetReference(all)) {
                    var charCount = Encoding.UTF8.GetCharCount(buffer, endLine + separatorLenght);
                    fixed (char* chars = stackalloc char[charCount]) {
                        var count = Encoding.UTF8.GetChars(buffer, endLine + separatorLenght, chars, charCount);
                        var line1 = new Span<char>(chars, count);

                        var test = new string(line1.ToArray());
                    }
                }
                
                //var slice = all.Slice(endLine + separatorLenght);
                //var charCount = Encoding.UTF8.GetCharCount(slice);
                //var chars = new Span<char>(new char[charCount]);
                //Encoding.UTF8.GetChars(slice, chars);
                //line = (ReadOnlySpan<char>)chars;

            //    var test = new string(line.ToArray());

                all = all.Slice(endLine + separatorLenght);
                continue;

                if (line.StartsWith(comm) || line.IsWhiteSpace()) {
                    continue;
                }

                var part = line.Slice(2, line.Length - 2).Trim();
                if (line.StartsWith(group)) {
                    var names = part.Trim().ToString().SplitOnWhitespace();
                    groupname = string.Join(" ", names);//[0].ToString();
                    var key = string.Join(" ", names.Take(names.Length - 1));//[0].ToString();
                    current = fullGeometry.CreatePart(groupname);
                } else if (line.StartsWith(texture)) {
                    //vt u v w
                    // u is the value for the horizontal direction of the texture.
                    // v is an optional argument.
                    // v is the value for the vertical direction of the texture.The default is 0.
                    // w is an optional argument.
                    // w is a value for the depth of the texture.The default is 0.

                    SplitVertex(ref part, floats, 2);
                    var v = new Vector2(floats[0], 1 - floats[1]);
                    current.AddTextureCoor(ref v);
                } else if (line.StartsWith(normal)) {
                    SplitVertex(ref part, floats);
                    var v = new Vector3(floats[0], floats[1], floats[2]);
                    //current.AddNormal(ref v);
                } else if (line.StartsWith(vector)) {
                    try {
                        SplitVertex(ref part, floats);
                        var v = new Vector3(floats[0], floats[1], floats[2]);
                        current.AddPosition(ref v);
                    } catch (Exception ex) {
                        ex.ToString();
                    }
                } else if (line.StartsWith(face)) {
                    var val = SplitFace(ref part);
                    if (new HashSet<int>(val).Count != 3) {

                    }
                    //FullGeometry1.Indices.AddRange(val);
                    try {
                        current.AddTriangle(val);
                    } catch (Exception ex) {
                        //TODO collect info for displaing in output 
                    }
                } else if (line.StartsWith(material)) {
                    //mtllib filename.mat
                    var end = line.IndexOf(spaceChar);
                    var filename = line.Slice(end + 1, line.Length - end - 1);
                    LoadMaterial(ref filename);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static void SplitVertex(ref ReadOnlySpan<char> span, float[] val, int count = 3) {
            var index = 0;
            while (index < count) {
                val[index] = 0;
                var end = span.IndexOf(spaceChar);
                if (end == -1) {
                    end = span.Length;
                }
                var part = span.Slice(0, end).Trim();
                
                val[index] = float.Parse(part, NumberStyles.Float, CultureInfo.InvariantCulture);
                index++;
                span = span.Slice(end, span.Length - end).Trim();
            }
        }

        static List<int> SplitFace(ref ReadOnlySpan<char> span) {
            var val = new List<int>();
            var index = 0;
            while (!span.IsWhiteSpace()) {
                var end = span.IndexOf(spaceChar);
                if (end == -1) {
                    end = span.Length;
                }
                var part = span.Slice(0, end).Trim();
                var sep = part.IndexOf('/');
                if (sep != -1) {
                    part = span.Slice(0, sep).Trim();
                }
                
                val.Add(int.Parse(part, NumberStyles.Integer, CultureInfo.InvariantCulture) - 1);
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

        void LoadMaterial(ref ReadOnlySpan<char> part) {
            var splitBySpace = part.IndexOf(spaceChar);
            if (splitBySpace != -1) {
                part = part.Slice(splitBySpace, part.Length - splitBySpace);
            }
            var fileName = part.ToString();
            var fullPath = Path.Combine(textureDir.FullName, fileName);
            if (!File.Exists(fullPath)) {
                return;
            }
            var newmtl = new ReadOnlySpan<char>(newmtlChars);
            var diffuseMap = new ReadOnlySpan<char>(diffuseMapChars);//map_Kd

            using (var mreader = new StreamReader(fullPath)) {
                while (!mreader.EndOfStream) {
                    var span = mreader.ReadLine().AsSpan();
                    if (span.StartsWith(newmtl)) {
                        //ignore for now, need to create list of materials
                    } else if (span.StartsWith(diffuseMap)) {
                        var end = span.IndexOf(spaceChar);
                        var filename = span.Slice(end + 1, span.Length - end - 1);
                        MaterialFilePath = new FileInfo(Path.Combine(textureDir.FullName, filename.ToString()));

                        return; //support only one material for now
                    }
                }
            }
        }
    }
}
