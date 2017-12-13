using D3DLab.Core.Common;

namespace D3DLab.Core.Test {
    public static class ViewportBuilder {

        public sealed class PerfomanceComponent : D3DComponent {
            public double ElapsedMilliseconds { get; set; }
            public double FPS { get; set; }


            public override string ToString() {
                return $"Perfomance[ElapsedMilliseconds:{ElapsedMilliseconds} FPS:{FPS}]";
            }
        }

        public sealed class InputInfoComponent : D3DComponent {
            public double EventCount { get; set; }


            public override string ToString() {
                return $"InputInfo[EventCount:{EventCount}";
            }
        }

        public static Entity Build(IEntityManager context) {
            var view = context.CreateEntity("Viewport");

            view.AddComponent(new PerfomanceComponent());
            view.AddComponent(new InputInfoComponent());

            return view;
        }
    }


}
