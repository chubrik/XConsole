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

        public XConsolePosition Write(params string[] values)
        {
            return values.Length > 0
                ? XConsole.WriteToPosition(values, this)
                : this;
        }
    }
}
