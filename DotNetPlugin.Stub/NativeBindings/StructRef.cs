using System;
using System.Runtime.CompilerServices;

namespace DotNetPlugin.NativeBindings
{
    /// <remarks>
    /// Safe to use with <see href="https://docs.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types">blittable types</see> only!
    /// </remarks>
    [Serializable]
    public readonly struct StructRef<T> where T : unmanaged
    {
        private readonly IntPtr _intPtr;

        public StructRef(IntPtr intPtr)
        {
            _intPtr = intPtr;
        }

        public bool HasValue => _intPtr != IntPtr.Zero;

        public ref readonly T Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _intPtr.ToStructUnsafe<T>(); }
    }
}
