using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace D3DLab.Std.Engine.Core.Render {
    public abstract class EngineCore {
        static readonly double total = TimeSpan.FromSeconds(1).TotalMilliseconds;
        static readonly double oneFrameMilliseconds = (total / 60);
        //static double _desiredFrameLengthSeconds = 1.0 / 60.0;

        public IAppWindow Window { get; }
        IEntityRenderNotify notify;

        Task loopTask;
        readonly CancellationTokenSource tokensource;
        readonly CancellationToken token;

        public IContextState Context { get; }

        public EngineCore(IAppWindow window, IContextState context) {
            Context = context;
            this.Window = window;

            tokensource = new CancellationTokenSource();
            token = tokensource.Token;
        }

        protected abstract void Initializing();

        public void Run(IEntityRenderNotify notify) {
            this.notify = notify;
            Initializing();
            loopTask = Task.Run((Action)Loop);
        }

        protected virtual void OnSynchronizing() {

        }

        void Loop() {
            System.Threading.Thread.CurrentThread.Name = "Game Loop";
            var imanager = Window.InputManager;

            //first synchronization
            Context.GetEntityManager().Synchronize();
            imanager.Synchronize();            

            var speed = new Stopwatch();
            var engineInfoTag = Context.GetEntityManager().GetEntities()
                    .Single(x => x.Has<EngineInfoBuilder.PerfomanceComponent>()).Tag;

            double millisec = oneFrameMilliseconds;
            while (Window.IsActive && !token.IsCancellationRequested) {
                speed.Restart();

                imanager.Synchronize();
                OnSynchronizing();

                var eman = Context.GetEntityManager();

                Rendering(eman, imanager, millisec);

                millisec = speed.ElapsedMilliseconds;

                if (millisec < oneFrameMilliseconds) {
                    Thread.Sleep((int)(oneFrameMilliseconds - millisec));
                }
                speed.Stop();

                millisec = speed.ElapsedMilliseconds;

                var perfomance = eman.GetEntity(engineInfoTag)
                    .GetComponent<EngineInfoBuilder.PerfomanceComponent>();

                perfomance.Update(millisec, (int)(total / millisec));
                //Debug.WriteLine($"FPS {(int)(total / speed.ElapsedMilliseconds)} / {speed.ElapsedMilliseconds} ms");

                notify.NotifyRender(eman.GetEntities().ToArray());
            }

            Window.InputManager.Dispose();
            Context.Dispose();
        }

        void Rendering(IEntityManager emanager, IInputManager imanager, double millisec) {
            var ishapshot = imanager.GetInputSnapshot();

            if (!ishapshot.Events.Any() && !emanager.HasChanges) {//no input/changes no rendering 
                return;
            }

            emanager.Synchronize();

            try {
                var snapshot = new SceneSnapshot(Window, Context, ishapshot, TimeSpan.FromMilliseconds(millisec));
                foreach (var sys in Context.GetSystemManager().GetSystems()) {
                    sys.Execute(snapshot);
                }
            } catch (Exception ex) {
                ex.ToString();
                throw ex;
            }
        }

        public virtual void Dispose() {
            if (loopTask.Status == TaskStatus.Running) {
                tokensource.Cancel();
                loopTask.Wait();
            }
        }
    }
}
