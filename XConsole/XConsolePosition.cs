namespace System
{
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
        public void Write() => throw new NotSupportedException("At least one argument should be specified");

        public XConsolePosition Write(params string?[] values)
        {
            return values.Length > 0
                ? XConsole.WriteToPosition(this, values)
                : this;
        }

        [Obsolete("At least one argument should be specified", error: true)]
        public void TryWrite() => throw new NotSupportedException("At least one argument should be specified");

        public XConsolePosition? TryWrite(params string?[] values)
        {
            if (values.Length > 0)
                try { return XConsole.WriteToPosition(this, values); }
                catch { return null; }

            return this;
        }

        #region Deprecated

        // v1.0.4
        [Obsolete("Property is deprecated, use InitialTop property instead.")]
        public int Top => InitialTop;

        // v1.0.4
        [Obsolete("Method is deprecated, use ActualTop property instead.")]
        public bool TryGetShiftedTop(out int shiftedTop)
        {
            var actualTop = ActualTop;
            shiftedTop = actualTop ?? default;
            return actualTop != null;
        }

        #endregion
    }
}
