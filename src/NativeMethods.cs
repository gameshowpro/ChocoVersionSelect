namespace ChocoVersionSelect;

internal static partial class NativeMethods
{
    [LibraryImport("dwmapi.dll")]
    internal static partial int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    internal const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    internal const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongW")]
    internal static partial int GetWindowLong(IntPtr hWnd, int nIndex);
    
    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongW")]
    internal static partial int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    internal const int GWL_STYLE = -16;

    internal const int WS_MAXIMIZEBOX = 0x10000; //maximize button
    internal const int WS_MINIMIZEBOX = 0x20000; //minimize button
}
