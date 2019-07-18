using BepuPhysics;
using BepuPhysics.Collidables;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Physics.Engine.Bepu {
    class BepuDynamicPhysicalComponent : PhysicalComponent {

        public readonly Std.Engine.Core.Utilities.BoundingBox box;

        public BepuDynamicPhysicalComponent(Std.Engine.Core.Utilities.BoundingBox box) {
            this.box = box;
        }

        public override void Dispose() {

            base.Dispose();
        }

        internal override void ConstructBody(Simulation simulation) {
            //var sphere = new Sphere(box.Size().X);
            var size = box.Size();
            var fbox = new Box(size.X, size.Y, size.Z);
            fbox.ComputeInertia(1, out var sphereInertia);

            var t = simulation.Shapes.Add(fbox);
            BodyIndex = t.Index;

            simulation.Bodies.Add(BodyDescription.CreateDynamic(
                box.GetCenter(),
                sphereInertia,
                new CollidableDescription(t, 0.1f),
                new BodyActivityDescription(0.01f)));

            IsConstructed = true;
            //var position = new Vector3();
            //var orientation = BepuUtilities.Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), 0);
            //var pose = new RigidPose(position, orientation);
            //var ringBoxShape = new Box(0.5f, 1, 3);
            //ringBoxShape.ComputeInertia(1, out var ringBoxInertia);
            //var boxDescription = BodyDescription.CreateDynamic(new Vector3(), ringBoxInertia,
            //    new CollidableDescription(simulation.Shapes.Add(ringBoxShape), 0.1f),
            //    new BodyActivityDescription(0.01f));

            //Data.Body = simulation.Bodies.Add(BodyDescription.CreateDynamic(pose, bodyInertia, new CollidableDescription(bodyShape, 0.1f), new BodyActivityDescription(0.01f)));

        }
    }
}
