using D3DLab.Core.Common;
using D3DLab.Core.Components;
using D3DLab.Core.Entities;
using D3DLab.Std.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D3DLab.Core.Context {

    
    


    public class Viewport : IViewportContext {
        public Graphics Graphics { get; set; }
        public World World { get; set; }        
    }

}
