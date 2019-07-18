using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Collections;
using BepuUtilities.Memory;
using D3DLab.Physics.Engine.Bepu;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace D3DLab.Physics.Engine {
    
    public static class PhysicalComponentFactory {
        public static PhysicalComponent Create(Std.Engine.Core.Utilities.BoundingBox box) {
            return new BepuDynamicPhysicalComponent(box);
        }
        public static PhysicalComponent CreateStatic(Std.Engine.Core.Utilities.BoundingBox box) {
            return new BepuStaticPhysicalComponent(box);
        }
    }
}

