using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.ECS.Systems;
using D3DLab.Std.Engine.Core.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace D3DLab.Toolkit.D3Objects {
    public class CameraObject : SingleGameObject {
        static int cameras = 0;

        public CameraObject(ElementTag tag, string descr) : base(tag, descr) { }

        public static CameraObject CreatePerspective<TRenderSystem, TToolkitFrameProperties>(IContextState context)
            where TRenderSystem : BaseRenderSystem<TToolkitFrameProperties>
            where TToolkitFrameProperties : IToolkitFrameProperties {

            IEntityManager manager = context.GetEntityManager();
            var cameraTag = new ElementTag("CameraEntity_" + Interlocked.Increment(ref cameras));

            var obj = new CameraObject(cameraTag, "PerspectiveCamera");

            manager.CreateEntity(cameraTag)
                   //.AddComponent(new OrthographicCameraComponent(Window.Width, Window.Height));
                   .AddComponent(new PerspectiveCameraComponent());

            {//entities ordering 
                context.EntityOrder
                       .RegisterOrder<TRenderSystem>(cameraTag, 0)
                       .RegisterOrder<DefaultInputSystem>(cameraTag, 0);
            }

            return obj;
        }

        public static CameraObject CreateOrthographic<TRenderSystem, TToolkitFrameProperties>(IContextState context, IAppWindow win) 
            where TRenderSystem : BaseRenderSystem<TToolkitFrameProperties> 
            where TToolkitFrameProperties : IToolkitFrameProperties {

            var manager = context.GetEntityManager();
            var cameraTag = new ElementTag("CameraEntity_" + Interlocked.Increment(ref cameras));

            var obj = new CameraObject(cameraTag, "OrthographicCamera");

            manager.CreateEntity(cameraTag)
                   .AddComponent(new OrthographicCameraComponent(win.Width, win.Height));

            {//entities ordering 
                context.EntityOrder
                       .RegisterOrder<TRenderSystem>(cameraTag, 0)
                       .RegisterOrder<DefaultInputSystem>(cameraTag, 0);
            }

            return obj;
        }

    }
}
