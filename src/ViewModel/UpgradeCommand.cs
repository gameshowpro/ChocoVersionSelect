using ChocoVersionSelect.Model;
using System.Windows.Input;

namespace ChocoVersionSelect.ViewModel;
public class UpgradeCommand(
    Func<PackageVersion, Task> execute
    ) : IAsyncCommand<PackageVersion>
{
    private readonly Func<PackageVersion, Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private bool _parentEnable;
    internal void ParentSetEnable(bool enable)
    {
        if (enable != _parentEnable)
        {
            _parentEnable = enable;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public virtual bool CanExecute(PackageVersion parameter)
        => _parentEnable && !parameter.IsCurrent;

    public event EventHandler? CanExecuteChanged;

    ///<summary>
    ///Defines the method to be called when the command is invoked.
    ///</summary>
    ///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
    public async Task ExecuteAsync(PackageVersion parameter)
    {
        if (CanExecute(parameter))
        {
            await _execute(parameter);
        }
    }

    #region ICommand
    bool ICommand.CanExecute(object? parameter)
        => parameter is null || CanExecute((PackageVersion)parameter);

    void ICommand.Execute(object? parameter)
    {
        if (parameter != null)
        {
            FireAndForget(ExecuteAsync((PackageVersion)parameter));
        }
    }
    #endregion
    public static async void FireAndForget(Task task)
        => await task;
}
