using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Std.Engine.Core.MeshFormats {
    public class ObjGroupInfo {
        public string Name;
        public int StartIndex;
        public int Count;
    }

    //public class ObjGroupsInfo {
    //    public readonly List<ObjGroupInfo> Faces;
    //    public readonly List<ObjGroupInfo> Indexes;

    //    public ObjGroupsInfo() {
    //        Faces = new List<ObjGroupInfo>();
    //        Indexes = new List<ObjGroupInfo>();
    //    }
    //}


    public class ObjGroupsComponent : GraphicComponent {
        public readonly List<ObjGroupInfo> Faces;
        public readonly List<ObjGroupInfo> Indexes;

        public ObjGroupsComponent() {
            Faces = new List<ObjGroupInfo>();
            Indexes = new List<ObjGroupInfo>();
        }
    }

}
