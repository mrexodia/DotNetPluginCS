using System;
using System.IO;
using System.Text;

namespace DotNetPlugin.Bindings.SDK
{
    public sealed class PLogTextWriter : TextWriter
    {
        public static readonly PLogTextWriter Default = new PLogTextWriter();

        private PLogTextWriter()
        {
            NewLine = "\n";
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value) =>
            Write(value.ToString());

        public override void Write(char[] buffer, int index, int count) =>
            Write(new string(buffer, index, count));

        public override void Write(string value) =>
            Write(value, Array.Empty<object>());

        public override void Write(string format, object arg0) =>
            Write(format, new[] { arg0 });

        public override void Write(string format, object arg0, object arg1) =>
            Write(format, new[] { arg0, arg1 });

        public override void Write(string format, object arg0, object arg1, object arg2) =>
            Write(format, new[] { arg0, arg1, arg2 });

        public override void Write(string format, params object[] args) =>
            Plugins._plugin_logprint(string.Format(format, args));

        public override void WriteLine(char value) =>
            WriteLine(value.ToString());

        public override void WriteLine(char[] buffer, int index, int count) =>
            WriteLine(new string(buffer, index, count));

        public override void WriteLine(string value) =>
            WriteLine(value, Array.Empty<object>());

        public override void WriteLine(string format, object arg0) =>
            WriteLine(format, new[] { arg0 });

        public override void WriteLine(string format, object arg0, object arg1) =>
            WriteLine(format, new[] { arg0, arg1 });

        public override void WriteLine(string format, object arg0, object arg1, object arg2) =>
            WriteLine(format, new[] { arg0, arg1, arg2 });

        public override void WriteLine(string format, params object[] args) =>
            Write(format + NewLine, args);
    }
}
