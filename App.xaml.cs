namespace ChocoVersionSelect;

public partial class App : Application
{
    private readonly static CancellationTokenSource s_cancellationTokenSource = new();

    protected override async void OnStartup(StartupEventArgs e)
    {
        CommandLineArgs? args = CommandLineParser.Parse(e.Args);
        Window window;
        Sys? sys = null;
        if (args == null)
        {
            window = new View.Syntax();
        }
        else
        {
            sys = new(args, s_cancellationTokenSource.Token);
            window = new View.MainWindow() { DataContext = sys };
            WindowCustomization.SetWin32Style(window, false);
        }
        DarkModeSupport.Initialize(this, window);
        window.Show();
        window.Closed += Window_Closed;
        if (sys != null)
        {
            await sys.DoRefresh();
        }
    }

    protected virtual void Window_Closed(object? sender, EventArgs e)
    {
        s_cancellationTokenSource.Cancel();
        Current.Shutdown();
    }
}
