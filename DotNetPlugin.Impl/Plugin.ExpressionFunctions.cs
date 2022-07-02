namespace DotNetPlugin
{
    partial class Plugin
    {
        [ExpressionFunction]
        public static nuint DotNetAdd(nuint a, nuint b)
        {
            return a + b;
        }
    }
}
