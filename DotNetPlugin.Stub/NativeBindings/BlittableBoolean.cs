using System;

namespace DotNetPlugin.NativeBindings
{
    // Based on: https://aakinshin.net/posts/blittable/#boolean
    [Serializable]
    public struct BlittableBoolean
    {
        private byte _byteValue;

        public bool Value
        {
            get => Convert.ToBoolean(_byteValue);
            set => _byteValue = Convert.ToByte(value);
        }

        public static explicit operator BlittableBoolean(bool value) => new BlittableBoolean { Value = value };

        public static implicit operator bool(BlittableBoolean value) => value.Value;
    }
}
