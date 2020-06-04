using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS {
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// DO NOT ADD {set} in this interface because it is used for 'readonly struct'
    /// </remarks>
    public interface IGraphicComponent : IDisposable {
        ElementTag Tag { get; }
        bool IsValid { get; }
        bool IsModified { get; }
        bool IsDisposed { get; }
    }

    public interface IFlyweightGraphicComponent : IDisposable {
        ElementTag Tag { get; }
        bool IsModified { get; set; }
        bool IsDisposed { get; }
    }
}
