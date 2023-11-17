namespace ChocoVersionSelect
{
    internal static class WindowCustomization
    {
        internal static void SetWin32Style(Window window, bool maximizeBox)
        {
            window.SourceInitialized += Window_SourceInitialized;
            void Window_SourceInitialized(object? sender, EventArgs e)
            {
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    int previousStyle = GetWindowLong(hwnd, GWL_STYLE);
                    _ = SetWindowLong(hwnd, GWL_STYLE, previousStyle & (maximizeBox ? WS_MAXIMIZEBOX : ~WS_MAXIMIZEBOX));
                }
            }
        }
    }
}
