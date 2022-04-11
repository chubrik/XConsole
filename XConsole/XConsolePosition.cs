namespace System
{
    public struct XConsolePosition
    {
        public readonly int Left;
        public readonly int Top;
        internal readonly long ShiftTop;

        public XConsolePosition(int left, int top)
        {
            Left = left;
            Top = top;
            ShiftTop = XConsole.ShiftTop;
        }

        internal XConsolePosition(int left, int top, long shiftTop)
        {
            Left = left;
            Top = top;
            ShiftTop = shiftTop;
        }

        public bool TryGetShiftedTop(out int shiftedTop) => XConsole.TryGetPositionShiftedTop(this, out shiftedTop);

        [Obsolete("At least one argument should be specified", error: true)]
        public void Write() => throw new NotSupportedException("At least one argument should be specified");

        public XConsolePosition Write(params string?[] values)
        {
            return values.Length > 0
                ? XConsole.WriteToPosition(values, this)
                : this;
        }
    }
}
