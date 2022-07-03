using System;
using System.Runtime.CompilerServices;

namespace DotNetPlugin.NativeBindings
{
    public readonly ref struct StructRef<T> where T : unmanaged
    {
        private readonly IntPtr _intPtr;

        internal StructRef(IntPtr intPtr)
        {
            _intPtr = intPtr;
        }

        public bool HasValue => _intPtr != IntPtr.Zero;

        public ref readonly T Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _intPtr.ToStructUnsafe<T>(); }
    }
}
