using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.ECS.Ext;
using D3DLab.Toolkit.Components;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;

namespace D3DLab.Toolkit.D3Objects {
    public class LightObject : SingleGameObject {
        static float lightpower = 1;
        static int lights = 0;

        public LightObject(ElementTag tag, string desc) : base(tag, desc) { }


        #region Creators

        public static LightObject CreateFollowCameraDirectLight(IEntityManager manager, Vector3 direction, float intensity = 0.2f) {// ,
            var tag = new ElementTag("DirectionLight_" + Interlocked.Increment(ref lights));
            manager.CreateEntity(tag)
                   .AddComponents(
                       LightComponent.CreateDirectional(intensity, 2, direction),
                       ColorComponent.CreateDiffuse(new Vector4(1, 1, 1, 1)),
                       FollowCameraDirectLightComponent.Create()
                   );

            return new LightObject(tag, "DirectionLight");
        }

        public static LightObject CreatePointLight(IEntityManager manager, Vector3 position) {// 
            var tag = new ElementTag("PointLight_" + Interlocked.Increment(ref lights));

            manager.CreateEntity(tag)
                 .AddComponents(
                     LightComponent.CreatePoint(0.4f, 1, position),
                     ColorComponent.CreateDiffuse(new Vector4(1, 1, 1, 1))
                 );

            return new LightObject(tag, "PointLight");
        }

        public static LightObject CreateAmbientLight(IEntityManager manager, float intensity = 0.4f) {
            var tag = new ElementTag("AmbientLight_" + Interlocked.Increment(ref lights));
            var sv4 = SharpDX.Color.White.ToVector4();

            manager.CreateEntity(tag)
                   .AddComponents(
                           LightComponent.CreateAmbient(intensity, 0),
                           ColorComponent.CreateDiffuse(new Vector4(sv4.X, sv4.Y, sv4.Z, sv4.W))
                       );

            return new LightObject(tag, "AmbientLight");
        }

        public static LightObject CreateDirectionLight(IEntityManager manager, Vector3 direction, float intensity) {// ,
            var tag = new ElementTag("DirectionLight_" + Interlocked.Increment(ref lights));
            manager.CreateEntity(tag)
                   .AddComponents(
                       LightComponent.CreateDirectional(intensity, 2, direction),
                       ColorComponent.CreateDiffuse(new Vector4(1, 1, 1, 1))
                   );

            return new LightObject(tag, "DirectionLight");
        }

        #endregion
        /*
        Std.Engine.Core.GeometryGameObject debugVisualObject;
        public override void ShowDebugVisualization(IEntityManager manager) {
            if (debugVisualObject.IsNotNull()) {
                debugVisualObject.Show(manager);
                //     coordinateSystemObject.Show(manager);
                return;
            }

            var entity = manager.GetEntity(Tag);
            var l = entity.GetComponent<LightComponent>();
            var c = entity.GetComponent<ColorComponent>();

            var center = l.Position;
            //switch (l.Type) {
            //    case LightTypes.Point:
            //        debugVisualObject = SphereGameObject.Create(manager, new SphereGameObject.Data {
            //            Center = center,
            //            Color = V4Colors.Red,// c.Color * l.Intensity
            //        });
            //        break;
            //}

            //coordinateSystemObject = CoordinateSystemLinesGameObject.Build(manager, center);

            base.ShowDebugVisualization(manager);
        }

        public override void HideDebugVisualization(IEntityManager manager) {
            debugVisualObject.Hide(manager);
            //   coordinateSystemObject.Hide(manager);

            base.HideDebugVisualization(manager);
        }

        public override void LookAtSelf(IEntityManager manager) {
            var entity = manager.GetEntity(Tag);
            var l = entity.GetComponent<LightComponent>();

            var com = new MoveCameraToTargetComponent { Target = Tag, TargetPosition = l.Position };

            manager.GetEntity(Tag).AddComponent(com);
        }
        */
    }
}
