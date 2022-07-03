using System;

namespace DotNetPlugin.NativeBindings.SDK
{
    // https://github.com/x64dbg/x64dbg/blob/development/src/bridge/bridgemain.h
    public sealed partial class Bridge : BridgeBase
    {
        [Serializable]
        public struct ICONDATA
        {
            public IntPtr data;
            public nuint size;
        }
    }
}
