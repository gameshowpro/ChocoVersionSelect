namespace ChocoVersionSelect;

internal static class DarkModeSupport
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string RegistryValueName = "AppsUseLightTheme";

    private enum WindowsTheme
    {
        Unknown,
        Light,
        Dark
    }

    private static Collection<ResourceDictionary>? s_resources;
    private static Window? s_window;
    private static IntPtr s_windowHandle;
    private static WindowsTheme s_windowsTheme;
    private static FrozenDictionary<WindowsTheme, ResourceDictionary>? s_themes;
    private static FrozenDictionary<WindowsTheme, Uri>? s_icons;

    internal static void Initialize(Application application, Window window)
    {
        s_resources = application.Resources.MergedDictionaries;
        s_themes = FrozenDictionary.ToFrozenDictionary<WindowsTheme, ResourceDictionary>(
            [
                new(WindowsTheme.Light, new ResourceDictionary() { Source = new Uri("/Themes/Light.xaml", UriKind.Relative)}),
                new(WindowsTheme.Dark, new ResourceDictionary() { Source = new Uri("/Themes/Dark.xaml", UriKind.Relative) }),
            ]);
        s_icons= FrozenDictionary.ToFrozenDictionary<WindowsTheme, Uri>(
        [
            new(WindowsTheme.Light, new Uri("pack://application:,,,/ChocoVersionSelect;component/Themes/Light.ico", UriKind.Absolute)),
            new(WindowsTheme.Dark, new Uri("pack://application:,,,/ChocoVersionSelect;component/Themes/Dark.ico", UriKind.Absolute)),
        ]);
        s_window = window;
        window.SourceInitialized += Window_SourceInitialized;
        _ = WatchTheme();
    }

    private static void Window_SourceInitialized(object? sender, EventArgs e)
    {
        s_windowHandle = new WindowInteropHelper(s_window).Handle;
        SetTitleBarTheme();
    }

    //Adapted from https://engy.us/blog/2018/10/20/dark-theme-in-wpf/
    private static bool WatchTheme()
    {
        string? userString = WindowsIdentity.GetCurrent()?.User?.Value;
        if (userString == null)
        {
            return false;
        }
        string query = string.Format(
            CultureInfo.InvariantCulture,
            @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
            userString,
            RegistryKeyPath.Replace(@"\", @"\\"),
            RegistryValueName);

        try
        {
            ManagementEventWatcher watcher = new (query);
            watcher.EventArrived += (sender, args) =>
            {
                UpdateWindowsTheme();
                
            };
            watcher.Start();
        }
        catch (Exception)
        {
            return false;
        }
        UpdateWindowsTheme();
        return true;
    }

    private static void UpdateWindowsTheme()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
        object? registryValueObject = key?.GetValue(RegistryValueName);
        WindowsTheme newTheme = (registryValueObject is int valueInt && valueInt > 0) ? WindowsTheme.Light : WindowsTheme.Dark;
        if (newTheme != s_windowsTheme)
        {
            s_windowsTheme = newTheme;
            WindowsThemeChanged();
        }
    }
    private static void WindowsThemeChanged()
    {
        SetTitleBarTheme();
        if (s_resources != null && s_themes?.TryGetValue(s_windowsTheme, out ResourceDictionary? theme) == true)
        {
            s_resources.Clear();
            s_resources.Add(theme);
        }
        if (s_window != null && s_icons?.TryGetValue(s_windowsTheme, out Uri? uri) == true)
        {
            if (s_window.Dispatcher.CheckAccess())
            {
                s_window.Icon = BitmapFrame.Create(uri);
            }
            else
            {
                s_window.Dispatcher.BeginInvoke(new Action(() => s_window.Icon = BitmapFrame.Create(uri)));
            }
        }
    }

    internal static bool SetTitleBarTheme()
    {
        if (s_windowHandle == IntPtr.Zero)
        {
            return false;
        }
        if (Environment.OSVersion.Version.Major >= 10)
        {
            int build = Environment.OSVersion.Version.Build;
            if (build >= 17763)
            {
                int attribute = build >= 18985 ? DWMWA_USE_IMMERSIVE_DARK_MODE : DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;

                int requestDarkMode = s_windowsTheme == WindowsTheme.Dark ? 1 : 0;
                return DwmSetWindowAttribute(s_windowHandle, attribute, ref requestDarkMode, sizeof(int)) == 0;
            }
        }
        return false;
    }
}
