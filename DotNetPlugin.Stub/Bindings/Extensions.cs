using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace DotNetPlugin.Bindings
{
    public static class Extensions
    {
        static Extensions()
        {
#if AMD64
            Debug.Assert(IntPtr.Size == 8);
            toPtrStringFormat = "X16";
#else
            Debug.Assert(IntPtr.Size == 4);
            toPtrStringFormat = "X8";
#endif
        }

        private static readonly string toPtrStringFormat;

        public static string ToHexString(this IntPtr intPtr) =>
            intPtr.ToString("X");

        public static string ToHexString(this UIntPtr intPtr) =>
            ((nint)(nuint)intPtr).ToHexString();

        public static string ToPtrString(this IntPtr intPtr) =>
            intPtr.ToString(toPtrStringFormat);

        public static string ToPtrString(this UIntPtr intPtr) =>
            ((nint)(nuint)intPtr).ToPtrString();

        private static unsafe long GetCStrLength(byte* ptr)
        {
            byte* endPtr = ptr;

            for (; *endPtr != 0; endPtr++) { }

            return endPtr - ptr;
        }

        private static unsafe long GetCStrLength(byte* ptr, int size)
        {
            byte* endPtr = ptr;

            for (; size > 0 && *endPtr != 0; size--, endPtr++) { }

            return endPtr - ptr;
        }

        public static unsafe string MarshalToStringUTF8(this IntPtr buffer, int bufferSize)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            if (buffer == IntPtr.Zero)
                return null;

            var bufferPtr = (byte*)buffer.ToPointer();
            var length = checked((int)GetCStrLength(bufferPtr, bufferSize));

            return Encoding.UTF8.GetString(bufferPtr, length);
        }

        public static unsafe void MarshalToPtrUTF8(this string str, IntPtr buffer, int bufferSize)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            fixed (char* strPtr = str)
            {
                var bufferPtr = (byte*)buffer.ToPointer();
                var n = Encoding.UTF8.GetBytes(strPtr, str.Length, bufferPtr, bufferSize - 1);
                // makes sure that buffer contains a null terminated string
                *(bufferPtr + n) = 0;
            }
        }

        public static unsafe string MarshalToStringUTF8(this IntPtr buffer)
        {
            if (buffer == IntPtr.Zero)
                return null;

            // without unsafe it'd look like this...

            //using (var bytes = new MemoryStream())
            //{
            //    byte @byte;
            //    for (int i = 0; (@byte = Marshal.ReadByte(intPtr, i)) != 0; i++)
            //        bytes.WriteByte(@byte);

            //    return Encoding.UTF8.GetString(bytes.GetBuffer(), 0, (int)bytes.Position);
            //}

            // ...but here we want as few extra allocations and copying as possible

            var bufferPtr = (byte*)buffer.ToPointer();
            var length = checked((int)GetCStrLength(bufferPtr));

            return Encoding.UTF8.GetString(bufferPtr, length);
        }

        public static string[] MarshalToStringUTF8(this IntPtr[] intPtrs)
        {
            var strings = new string[intPtrs.Length];

            for (int i = 0; i < intPtrs.Length; i++)
                strings[i] = intPtrs[i].MarshalToStringUTF8();

            return strings;
        }

        public static T? ToStruct<T>(this IntPtr intPtr) where T : struct
        {
            if (intPtr == IntPtr.Zero)
                return null;

            return (T)Marshal.PtrToStructure(intPtr, typeof(T));
        }

        public static unsafe ref readonly T ToStructUnsafe<T>(this IntPtr intPtr) where T : unmanaged
        {
            if (intPtr == IntPtr.Zero)
                throw new ArgumentException("Invalid pointer.", nameof(intPtr));

            return ref *(T*)intPtr.ToPointer();
        }
    }
}
