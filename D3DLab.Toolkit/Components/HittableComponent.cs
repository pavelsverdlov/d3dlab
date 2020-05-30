using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Components {
    public struct HittableComponent : IGraphicComponent {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="priority">0 - max priority;uint.MaxValue - min priority</param>
        /// <returns></returns>
        public static HittableComponent Create(uint priority) {
            return new HittableComponent(priority) {
                IsValid = true,
                Tag = ElementTag.New(),
            };
        }
        public readonly uint PriorityIndex;

        HittableComponent(uint priorityIndex) : this() {
            PriorityIndex = priorityIndex;
        }

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            IsDisposed = true;
        }
    }
}
