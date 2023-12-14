namespace ChocoVersionSelect.Model;

internal enum PackageVersionSetType
{
    Single,
    BeforePrevious,
    BetweenCurrentAndLatest
}

internal enum WrapperState
{
    Idle,
    Upgrading,
    Downgrading,
    Refreshing
}

internal record PackageVersionSet(PackageVersionSetType PackageVersionSetType, ImmutableList<PackageVersion> PackageVersions)
{
    internal PackageVersionSet(PackageVersion packageVersion) :
        this(PackageVersionSetType.Single, [packageVersion])
    { }
}

internal class ChocoWrapper
{
    private const string PublishedTag = "Published: ";
    private readonly static TimeSpan s_timeout = TimeSpan.FromSeconds(30);
    private readonly CancellationToken _cancellationToken;
    private readonly CommandLineArgs _args;

    internal Channel<PackageVersionSet>? PackageVersions { get; private set; }
    /// <summary>
    /// Start reading data from Chocolatey immediately on another thread and return immediately.
    /// </summary>
    internal ChocoWrapper(CommandLineArgs args, Action<WrapperState> wrapperStateUpdateDelegate, CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _args = args;
        _wrapperStateUpdateDelegate = wrapperStateUpdateDelegate;
        _ = Task.Run(GetVersionData, cancellationToken);
    }

    private WrapperState _wrapperState;
    private readonly Action<WrapperState> _wrapperStateUpdateDelegate;
    private void UpdateWrapperState(WrapperState wrapperState)
    {
        if (wrapperState != _wrapperState)
        {
            _wrapperState = wrapperState;
            _wrapperStateUpdateDelegate(wrapperState);
        }
    }

    private string SourceOption()
        => _args.Source == null ? string.Empty : $"--source=\"{_args.Source}\" ";

    private static string AllowDowngradeOption(PackageVersion packageVersion)
        => packageVersion.IsDowngrade ? $"--allowDowngrade " : string.Empty;

    public async Task GetVersionData()
    {
        string dateFormat = CultureInfo.InstalledUICulture.DateTimeFormat.ShortDatePattern;
        UpdateWrapperState(WrapperState.Refreshing);
        PackageVersions = Channel.CreateUnbounded<PackageVersionSet>();
        PackageVersion? currentVersion = null;
        ImmutableList<PackageVersion>.Builder beforePrevious = ImmutableList.CreateBuilder<PackageVersion>();
        ImmutableList<PackageVersion>.Builder betweenCurrentAndLatest = ImmutableList.CreateBuilder<PackageVersion>();
        bool currentSeenInSearch = false;
        bool isLatest = true;
        bool previousWasCurrent = false;
        string? pendingVersionNumber = null;
        int listResult = await RunAsync(false, $"list {_args.PackageName} --exact --limitoutput", ProcessList);
        int searchResult = await RunAsync(false, $"search {_args.PackageName} {SourceOption()}--all-versions --exact --verbose", ProcessSearch);
        _ = PackageVersions.Writer.TryWrite(new(PackageVersionSetType.BetweenCurrentAndLatest, betweenCurrentAndLatest.ToImmutable()));
        _ = PackageVersions.Writer.TryWrite(new(PackageVersionSetType.BeforePrevious, beforePrevious.ToImmutable()));
        _ = PackageVersions.Writer.TryComplete();
        UpdateWrapperState(WrapperState.Idle);
        void ProcessList(string line)
        {
            if (currentVersion == null && line.StartsWith(_args.PackageName))
            {
                currentVersion = new(line[(_args.PackageName.Length + 1)..], true, false, false, false, null);
                _ = PackageVersions.Writer.TryWrite(new (currentVersion));
            }
        }
        void ProcessSearch(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                pendingVersionNumber = null;
            }
            else if (line.StartsWith(_args.PackageName))
            {
                int versionStart = _args.PackageName.Length + 1;
                int versionEnd = line.IndexOf(' ', versionStart);
                if (versionEnd == -1)
                {
                    versionEnd = line.Length;
                }
                pendingVersionNumber = line[versionStart..(versionEnd)];
            }
            else if (pendingVersionNumber != null)
            {
                int publishedPosition = line.LastIndexOf(PublishedTag);
                if (publishedPosition >= 0)
                {
                    if (DateTimeOffset.TryParseExact(line[(publishedPosition + PublishedTag.Length)..], dateFormat, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal, out DateTimeOffset publishDate))
                    {
                        bool isCurrent = currentVersion != null && pendingVersionNumber == currentVersion.Version;
                        PackageVersion packageVersion = new(pendingVersionNumber, isCurrent, previousWasCurrent, isLatest, currentSeenInSearch, publishDate);
                        currentSeenInSearch = currentSeenInSearch || isCurrent;
                        AddPackage(packageVersion);
                        previousWasCurrent = isCurrent;
                        isLatest = false;
                        pendingVersionNumber = null;
                    }
                }
            }
        }
        void AddPackage(PackageVersion packageVersion)
        {
            if (packageVersion.IsLatest || packageVersion.IsCurrent || packageVersion.IsPrevious) 
            {
                _ = PackageVersions.Writer.TryWrite(new(packageVersion));
            }
            else if (packageVersion.IsDowngrade)
            {
                beforePrevious.Add(packageVersion);
            }
            else
            {
                betweenCurrentAndLatest.Add(packageVersion);
            }
        }
    }

    public async Task<bool> Upgrade(PackageVersion packageVersion)
    {
        UpdateWrapperState(packageVersion.IsDowngrade ? WrapperState.Downgrading : WrapperState.Upgrading);
        int result = await RunAsync(true, $"upgrade {_args.PackageName} {SourceOption()}{AllowDowngradeOption(packageVersion)} --version {packageVersion.Version} --yes --limit-output", ProcessUpgrade);

        UpdateWrapperState(WrapperState.Idle);
        return result == 0;
        void ProcessUpgrade(string line)
        {

        }
    }

    private static async Task<int> RunAsync(bool elevated, string arguments, Action<string> processLine)
    {
        ProcessStartInfo startInfo = new("choco", arguments)
        {
            RedirectStandardOutput = !elevated,
            CreateNoWindow = true,
            UseShellExecute = elevated,
            Verb = elevated ? "runas" : string.Empty
        };
        using Process process = new() { StartInfo = startInfo, EnableRaisingEvents = true };
        // List of tasks to wait for a whole process exit
        List<Task> processTasks = [];

        TaskCompletionSource<object> processExitEvent = new ();
        process.Exited += (sender, args) =>
        {
            processExitEvent.TrySetResult(true);
        };
        processTasks.Add(processExitEvent.Task);

        var stdOutCloseEvent = new TaskCompletionSource<bool>();

        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data == null)
            {
                stdOutCloseEvent.TrySetResult(true);
            }
            else
            {
                processLine(e.Data);
            }
        };
        try
        {
            if (!process.Start())
            {
                return -1;
            }
        }
        catch (Exception)
        {
            return -1;
        }
        if (!elevated)
        {
            processTasks.Add(stdOutCloseEvent.Task);
            process.BeginOutputReadLine();
        }
        Task processCompletionTask = Task.WhenAll(processTasks);

        Task<Task> awaitingTask = Task.WhenAny(Task.Delay(s_timeout), processCompletionTask);

        // Let's now wait for something to end...
        if ((await awaitingTask.ConfigureAwait(false)) == processCompletionTask)
        {
            // -> Process exited cleanly
            return process.ExitCode;
        }
        else
        {
            // Timeout
            try
            {
                process.Kill();
            }
            catch
            {
                // ignored
            }
        }
        return -1;
    }
}
