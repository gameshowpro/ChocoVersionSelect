using System.Windows.Input;

namespace ChocoVersionSelect.ViewModel;
public class SimpleCommand(
    Func<Task> execute
    ) : IAsyncCommand<object?>
{
    private readonly Func<Task> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private bool _parentEnable;
    internal void ParentSetEnable(bool enable)
    {
        if (enable != _parentEnable)
        {
            _parentEnable = enable;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public virtual bool CanExecute(object? parameter)
        => _parentEnable;

    public event EventHandler? CanExecuteChanged;

    ///<summary>
    ///Defines the method to be called when the command is invoked.
    ///</summary>
    ///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
    public async Task ExecuteAsync(object? parameter)
    {
        if (CanExecute(parameter))
        {
            await _execute.Invoke();
        }
    }

    #region ICommand
    bool ICommand.CanExecute(object? parameter)
        => _parentEnable;

    void ICommand.Execute(object? parameter)
    {
        FireAndForget(ExecuteAsync(null));
    }
    #endregion
    public static async void FireAndForget(Task task)
        => await task;
}
