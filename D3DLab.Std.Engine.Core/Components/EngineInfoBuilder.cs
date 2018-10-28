using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Std.Engine.Core.Components {
    public static class EngineInfoBuilder {
        public sealed class PerfomanceComponent : GraphicComponent {
            public double ElapsedMilliseconds { get; set; }
            public double FPS { get; set; }


            public override string ToString() {
                return $"Perfomance[ElapsedMilliseconds:{ElapsedMilliseconds} FPS:{FPS}]";
            }
        }

        public sealed class InputInfoComponent : GraphicComponent {
            public double EventCount { get; set; }


            public override string ToString() {
                return $"InputInfo[EventCount:{EventCount}";
            }
        }

        public static GraphicEntity Build(IEntityManager context) {
            var view = context.CreateEntity(new ElementTag("EngineInfo"));

            view.AddComponent(new PerfomanceComponent())
                .AddComponent(new InputInfoComponent());

            return view;
        }
    }
}
