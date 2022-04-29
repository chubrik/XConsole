using System;

namespace Chubrik.XConsole
{
#if NET
    using System.Runtime.Versioning;
    [SupportedOSPlatform("windows")]
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
#endif
    public struct XConsolePosition
    {
        public readonly int Left;
        public readonly int InitialTop;
        internal readonly long ShiftTop;

        public XConsolePosition(int left, int top)
        {
            Left = left;
            InitialTop = top;
            ShiftTop = XConsole.ShiftTop;
        }

        internal XConsolePosition(int left, int top, long shiftTop)
        {
            Left = left;
            InitialTop = top;
            ShiftTop = shiftTop;
        }

        public int? ActualTop => XConsole.GetPositionActualTop(this);

        [Obsolete("At least one argument should be specified", error: true)]
        public void Write() => throw new InvalidOperationException();

        public XConsolePosition Write(params string?[] values)
        {
            return XConsole.WriteToPosition(this, values);
        }

        [Obsolete("At least one argument should be specified", error: true)]
        public void TryWrite() => throw new InvalidOperationException();

        public XConsolePosition? TryWrite(params string?[] values)
        {
            try
            {
                return XConsole.WriteToPosition(this, values);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }
    }
}
