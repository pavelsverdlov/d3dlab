using BepuPhysics;
using BepuPhysics.Collidables;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Physics.Engine.Bepu {
    class BepuStaticPhysicalComponent : PhysicalComponent {
        public readonly Std.Engine.Core.Utilities.BoundingBox box;

        public BepuStaticPhysicalComponent(Std.Engine.Core.Utilities.BoundingBox box) {
            this.box = box;
        }

        internal override void ConstructBody(Simulation simulation) {
            var size = box.Size();
            BodyIndex = simulation.Statics.Add(
                new StaticDescription(box.GetCenter(),
                new CollidableDescription(simulation.Shapes.Add(new Box(size.X, size.Y, size.Z)), 0.1f))
                );
            IsConstructed = true;
        }
    }
}
