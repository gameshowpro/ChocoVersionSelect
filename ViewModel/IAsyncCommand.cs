using System.Windows.Input;

namespace ChocoVersionSelect.ViewModel;

public interface IAsyncCommand<T> : ICommand
{
    Task ExecuteAsync(T parameter);
    bool CanExecute(T parameter);
}
