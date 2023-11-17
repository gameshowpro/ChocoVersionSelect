namespace ChocoVersionSelect;

public class Sys : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly ChocoWrapper _wrapper;
    private readonly CancellationToken _cancellationToken;
    private readonly Dispatcher _dispatcher;

    public DateTimeOffset TestDate { get => DateTimeOffset.Now; }

    internal Sys(CommandLineArgs args, CancellationToken cancellationToken)
    {
        _dispatcher = Dispatcher.CurrentDispatcher;
        PackageName = args.PackageName;
        _cancellationToken = cancellationToken;
        _wrapper = new(args, new(WrapperStateChanged), cancellationToken);
        UpgradeCommand = new(DoUpgrade);
        RefreshCommand = new(DoRefresh);
    }

    private void WrapperStateChanged(WrapperState wrapperState)
    {
        if (_dispatcher.CheckAccess())
        {
            DoWrapperStateChanged();
        }
        else
        {
            _dispatcher.BeginInvoke(new Action(DoWrapperStateChanged));
        }
        void DoWrapperStateChanged()
        {
            UpgradeCommand.ParentSetEnable(wrapperState == WrapperState.Idle);
            RefreshCommand.ParentSetEnable(wrapperState == WrapperState.Idle);
            IsRefreshing = wrapperState == WrapperState.Refreshing;
            IsUpgrading = wrapperState == WrapperState.Upgrading || wrapperState == WrapperState.Downgrading;
            UpgradeIsDowngrade = wrapperState == WrapperState.Downgrading;
            OnPropertyChanged(nameof(IsRefreshing));
            OnPropertyChanged(nameof(IsUpgrading));
            OnPropertyChanged(nameof(UpgradeIsDowngrade));
        }
    }

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new(propertyName));

    /// <summary>
    /// Expose incoming data to the UI as it arrives from the wrapper thread
    /// </summary>
    private async Task Populate()
    {
        OnPropertyChanged(nameof(Current));
        try
        {
            await foreach (PackageVersionSet packageVersionSet in _wrapper.PackageVersions!.Reader.ReadAllAsync(_cancellationToken))
            {
                switch (packageVersionSet.PackageVersionSetType)
                {
                    case PackageVersionSetType.Single:
                        PackageVersion packageVersion = packageVersionSet.PackageVersions[0];
                        if (packageVersion.IsCurrent == true)
                        {
                            Current = packageVersion;
                            OnPropertyChanged(nameof(Current));
                        }
                        if (packageVersion.IsLatest)
                        {
                            Latest = packageVersion;
                            OnPropertyChanged(nameof(Latest));
                        }
                        if (packageVersion.IsPrevious)
                        {
                            Previous = packageVersion;
                            OnPropertyChanged(nameof(Previous));
                        }
                        break;
                    case PackageVersionSetType.BeforePrevious:
                        BeforePrevious = packageVersionSet.PackageVersions;
                        OnPropertyChanged(nameof(BeforePrevious));
                        break;
                    case PackageVersionSetType.BetweenCurrentAndLatest:
                        BetweenCurrentAndLatest = packageVersionSet.PackageVersions;
                        OnPropertyChanged(nameof(BetweenCurrentAndLatest));
                        break;
                       
                }
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        UpgradeCommand.ParentSetEnable(true);
        RefreshCommand.ParentSetEnable(true);
    }

    private async Task DoUpgrade(PackageVersion packageVersion)
    {
        IsUpgrading = true;
        UpgradeIsDowngrade = packageVersion.IsDowngrade;
        bool upgraded = await _wrapper.Upgrade(packageVersion);
        IsUpgrading = false;
        OnPropertyChanged(nameof(IsUpgrading));
        if (upgraded)
        {
            await Task.WhenAll(_wrapper.GetVersionData(), Populate());
        }
    }

    internal async Task DoRefresh()
    {
        await Task.WhenAll(_wrapper.GetVersionData(), Populate());
    }

    public SimpleCommand RefreshCommand { get; }
    public UpgradeCommand UpgradeCommand { get; }
    public string PackageName { get; }
    public PackageVersion? Current { get; private set; }
    public PackageVersion? Previous { get; private set; }
    public PackageVersion? Latest { get; private set; }
    public ImmutableList<PackageVersion>? BeforePrevious {  get; private set; }
    public ImmutableList<PackageVersion>? BetweenCurrentAndLatest { get; private set; }
    public bool IsRefreshing { get; private set; }
    public bool IsUpgrading { get; private set; }
    public bool UpgradeIsDowngrade { get; private set; }
}