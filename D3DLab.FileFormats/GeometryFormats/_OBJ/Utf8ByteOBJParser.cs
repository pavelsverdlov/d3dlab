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
    /// https://en.wikipedia.org/wiki/Wavefront_.obj_file
    /// http://en.wikipedia.org/wiki/Material_Template_Library
    /// http://www.martinreddy.net/gfx/3d/OBJ.spec
    /// http://www.eg-models.de/formats/Format_Obj.html
    /// </remarks>
    public sealed class Utf8ByteOBJParser {
        enum TriangleTopology {
            TriangleList,
            TriangleFan
        }

        static readonly Encoding utf8;
        
        static readonly byte[] LFChars;
        static readonly byte[] commentChars;

        static readonly byte[] groupChars;
        static readonly byte[] vectorChars;
        static readonly byte[] textureChars;
        static readonly byte[] faceChars;

        static readonly byte[] materialChars;
        static readonly byte[] normalChars;

        static readonly byte CRChars;
        static readonly byte space;
        static readonly byte slash;

        static Utf8ByteOBJParser() {
            utf8 = Encoding.UTF8;
            space = Convert.ToByte(' ');
            slash = Convert.ToByte('/');

            CRChars = utf8.GetBytes(new[] { '\r' })[0];
            LFChars = utf8.GetBytes(new[] { '\n' });
            groupChars = utf8.GetBytes(new[] { 'g' });
            vectorChars = utf8.GetBytes(new[] { 'v' });
            textureChars = utf8.GetBytes(new[] { 'v', 't' });
            normalChars = utf8.GetBytes(new[] { 'v', 'n' });
            faceChars = utf8.GetBytes(new[] { 'f' });
            commentChars = utf8.GetBytes(new[] { '#' });
            materialChars = utf8.GetBytes(new[] { 'm', 't', 'l', 'l', 'i', 'b' });
        }

        public GroupGeometry3D FullGeometry { get; }
        public bool HasMTL => !string.IsNullOrWhiteSpace(MtlFileName);
        public string MtlFileName { get; private set; }

        public Utf8ByteOBJParser() {
            FullGeometry = new GroupGeometry3D();
        }
        public void Read(ReadOnlySpan<byte> bytes) {
            UnsafeRead(bytes);
        }
        
        public string GetMaterialFilePath(DirectoryInfo mtlDir, DirectoryInfo imgDir) {
            var path = Path.Combine(mtlDir.FullName, MtlFileName);
            if (!File.Exists(path)) {
                throw new Exception($"Path '{path}' does not exist.");
            }
            using (var fs = File.OpenRead(path)) {
                return GetMaterialFilePath(imgDir, new StreamReader(fs));
            }
        }

        public string GetMaterialFilePath(DirectoryInfo imgDir, StreamReader mtl) {
            const char LF = '\n';
            const char space = ' ';
            char[] newmtlChars = new[] { 'n', 'e', 'w', 'm', 't', 'l' };
            char[] diffuseMapChars = new[] { 'm', 'a', 'p', '_', 'K', 'd' };

            using (var reader = mtl) {
                var all = reader.ReadToEnd().AsSpan();
                while (!all.IsEmpty) {
                    var endLine = all.IndexOf(LF);
                    if (endLine == -1) {
                        endLine = all.Length;
                    }
                    var line = all.Slice(0, endLine).Trim();
                    if (line.StartsWith(newmtlChars)) {
                        //ignore for now, need to create list of materials
                    } else if (line.StartsWith(diffuseMapChars)) {//map_Kd
                        var end = line.IndexOf(space);
                        var filename = line.Slice(end + 1, line.Length - end - 1);
                        var materialFilePath = Path.Combine(imgDir.FullName,
                            filename.Trim().ToString()).Trim();
                        return materialFilePath; //support only one material for now
                    }

                    all = all.Slice(endLine).Trim();
                }
            }
            throw new Exception("Mtl is not correct");
        }

        unsafe void UnsafeRead(ReadOnlySpan<byte> all) {
            var groupname = "noname";
            var current = FullGeometry.CreatePart(groupname);
            var floats = new float[3];
            var triangleList = new int[4];
            var triangleFan = new int[6];

            while (!all.IsEmpty) {
                var endLine = all.IndexOf(LFChars);
                int separatorLenght = LFChars.Length;
                if (endLine == -1) {
                    endLine = all.Length;
                    separatorLenght = 0;
                }

                if (endLine == 0) {//has only CR_LF, ignore then
                    all = all.Slice(separatorLenght);
                    continue;
                }

                int cr = 0;
                if (all[endLine - 1] == CRChars) {//check 'CR LF'
                    cr = 1;
                }
                var line = all.Slice(0, endLine - cr);//ignore CR_LF or LF 

                all = all.Slice(endLine + separatorLenght);

                if (line.StartsWith(commentChars) || IsWhiteSpace(line)) {
                    continue;
                }
                //lenth 2 is default for OBJ format keys, just use it as a const
                var part = line.Slice(2, line.Length - 2).Trim(space);
                if (line.StartsWith(groupChars)) {
                    var names = GetString(part).Trim().SplitOnWhitespace();
                    groupname = string.Join(" ", names);//clean up group name from extra space
                    current = FullGeometry.CreatePart(groupname);
                } else if (line.StartsWith(textureChars)) {
                    //vt u v w
                    // u is the value for the horizontal direction of the texture.
                    // v is an optional argument.
                    // v is the value for the vertical direction of the texture.The default is 0.
                    // w is an optional argument.
                    // w is a value for the depth of the texture.The default is 0.

                    SplitVertex(ref part, floats, 2);
                    var v = new Vector2(floats[0], 1 - floats[1]); // '1 - ' specific fot OBJ format
                    current.AddTextureCoor(ref v);
                } else if (line.StartsWith(normalChars)) {
                    SplitVertex(ref part, floats);
                    var v = new Vector3(floats[0], floats[1], floats[2]);
                    current.AddNormal(ref v);
                } else if (line.StartsWith(vectorChars)) {
                    SplitVertex(ref part, floats);
                    var v = new Vector3(floats[0], floats[1], floats[2]);
                    current.AddPosition(ref v);
                    if (!part.IsEmpty) {//format 'v' line with colors
                                        //example: v -2.503583 6.779097 -5.350025 0.0 128.0 0.0
                        SplitVertex(ref part, floats);
                        v = new Vector3(floats[0], floats[1], floats[2]);
                        current.AddColor(ref v);
                    }
                    if (!part.IsEmpty) {
                        throw new NotSupportedException("Unexpected vertex format.");
                    }
                } else if (line.StartsWith(faceChars)) {
                    switch (SplitFace(part, triangleList, triangleFan)) {
                        case TriangleTopology.TriangleList:
                            current.AddTriangle(triangleList[0], triangleList[1], triangleList[2]);
                            //ignore triangleList[3] it is just buffer for triangle fan parsing 
                            break;
                        case TriangleTopology.TriangleFan:
                            current.AddTriangles(triangleFan);
                            current.AddTrianglesFun(triangleList[0], triangleList[1], triangleList[2], triangleList[3]);
                            break;
                    }
                } else if (line.StartsWith(materialChars)) {
                    //mtllib filename.mat
                    var end = line.IndexOf(space);
                    var filename = line.Slice(end + 1, line.Length - end - 1);
                    LoadMaterial(filename);
                }
            }

            FullGeometry.Build();
        }

        static bool IsWhiteSpace(in ReadOnlySpan<byte> span) {
            if (span.Length == 0) {
                return true;
            }
            for (var i = 0; i < span.Length; i++) {
                if (span[i] != space) {
                    return false;
                }
            }
            return true;
        }
        static unsafe string GetString(in ReadOnlySpan<byte> span) {
            fixed (byte* buffer = &MemoryMarshal.GetReference(span)) {
                var charCount = utf8.GetCharCount(buffer, span.Length);
                fixed (char* chars = stackalloc char[charCount]) {
                    var count = utf8.GetChars(buffer, span.Length, chars, charCount);
                    return new string(chars, 0, count);
                }
            }
        }
        static unsafe string GetStringFrom(in ReadOnlySpan<byte> all, int offset) {
            fixed (byte* buffer = &MemoryMarshal.GetReference(all)) {
                var charCount = utf8.GetCharCount(buffer, offset);
                fixed (char* chars = stackalloc char[charCount]) {
                    var count = utf8.GetChars(buffer, offset, chars, charCount);
                    var line1 = new Span<char>(chars, count);
                    return new string(line1.ToArray());
                }
            }
        }
        static unsafe string GetStringOfWholeMemory(in ReadOnlyMemory<byte> memory) {
            fixed (byte* buffer = &MemoryMarshal.GetReference(memory.Span)) {
                var charCount = utf8.GetCharCount(buffer, memory.Length);
                fixed (char* chars = stackalloc char[charCount]) {
                    var count = utf8.GetChars(buffer, memory.Length, chars, charCount);
                    var line1 = new Span<char>(chars, count);
                    return new string(line1.ToArray());
                }
            }
        }
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void SplitVertex(ref ReadOnlySpan<byte> span, float[] val, int count = 3) {
            var index = 0;
            while (index < count) {
                val[index] = 0;
                var end = span.IndexOf(space);
                if (end == -1) {
                    end = span.Length;
                }
                var part = span.Slice(0, end).Trim(space);

                if (!Utf8Parser.TryParse(part, out float value, out var _)) {
                    throw new Exception("Can't read float");
                }
                val[index] = value;
                index++;
                span = span.Slice(end, span.Length - end).Trim(space);
            }
        }
        static TriangleTopology SplitFace(ReadOnlySpan<byte> span, int[] triangleBuf, int[] triangleFanBuf) {
            //example: f v/vt/vn v/vt/vn v/vt/vn v/vt/vn
            var index = 0;
            while (!span.IsEmpty) {
                var end = span.IndexOf(space);
                if (end == -1) {
                    end = span.Length;
                }
                var part = span.Slice(0, end).Trim(space);
                var sep = part.IndexOf(slash);
                if (sep != -1) {//read only first element of v/vt/vn
                    part = span.Slice(0, sep).Trim(space);
                }
                if (!Utf8Parser.TryParse(part, out int value, out _)) {
                    throw new Exception("Can't read float");
                }
                triangleBuf[index] = value - 1;//' - 1' specific for OBJ format
                index++;
                span = span.Slice(end, span.Length - end).Trim(space);
            }
            if (index == 4) {//suport triangle fan format https://en.wikipedia.org/wiki/Triangle_fan
                //example: f 2 1 5 6
                index = 0;
                for (int i = 0; i + 2 < triangleBuf.Length; i++) {
                    triangleFanBuf[index] = triangleBuf[0];
                    triangleFanBuf[++index] = triangleBuf[i + 1];
                    triangleFanBuf[++index] = triangleBuf[i + 2];
                    ++index;
                }
                return TriangleTopology.TriangleFan;
            } else {
                return TriangleTopology.TriangleList;
            }

        }

        void LoadMaterial(ReadOnlySpan<byte> part) {
            var splitBySpace = part.IndexOf(space);
            if (splitBySpace != -1) {
                part = part.Slice(splitBySpace, part.Length - splitBySpace);
            }
            var mtlFileName = GetString(part.Trim(space));
            this.MtlFileName = mtlFileName;
        }

    }
}
