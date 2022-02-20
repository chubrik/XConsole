namespace System
{
    public struct XConsolePosition
    {
        public readonly int Left;
        public readonly int Top;

        public XConsolePosition(int left, int top)
        {
            Left = left;
            Top = top;
        }

        public XConsolePosition Write(params string[] values)
        {
            return values.Length > 0
                ? XConsole.WriteToPosition(values, left: Left, top: Top)
                : this;
        }
    }
}
