using D3DLab.ECS;
using D3DLab.Toolkit.Components;
using NUnit.Framework;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace D3DLab.NUnitTests {
    class D3DLabECSTests {
        class Notify : IManagerChangeNotify {
            public void NotifyAdd<T>(in T _object) {
            }
            public void NotifyRemove<T>(in T _object) {
            }
        }

        [Test]
        public void ENTITY_COMPS_MANAGRS_ASYNC_ADD_GET_TEST() {
            var sync = new ECS.Sync.RenderLoopSynchronizationContext();
            var manager = new EntityComponentManager(new Notify(), new EntityOrderContainer(), sync);
            
            var en = manager.CreateEntity(ElementTag.New());
            
            var sleep = TimeSpan.FromSeconds(1f / 60f);
            var runner = Task.Run(() => {
                while (true) {
                    Thread.Sleep(sleep);
                    sync.Synchronize(0);
                }
            });

            var addResult = Parallel.For(0, 100, index=> {
                en.AddComponent(RenderableComponent.AsPoints());
            });
            var removeResult = Parallel.For(0, 100, index => {
                en.RemoveComponent<RenderableComponent>();
            });

            var found = 0;
            while (true) {
                var com = en.GetComponent<RenderableComponent>();
                if (com.IsValid) {
                    found++;
                }
                if(found == 10) {
                    break;
                }
            }

        }
    }
}
