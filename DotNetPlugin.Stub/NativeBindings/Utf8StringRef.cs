using System;

namespace DotNetPlugin.NativeBindings
{
    [Serializable]
    public readonly struct Utf8StringRef
    {
        private readonly IntPtr _intPtr;

        public Utf8StringRef(IntPtr intPtr)
        {
            _intPtr = intPtr;
        }

        public string GetValue() => _intPtr.MarshalToStringUTF8();

        public static implicit operator string(Utf8StringRef value) => value.GetValue();
    }
}
