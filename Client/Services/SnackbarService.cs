using BravusApp.Client.Model;

namespace BravusApp.Client.Services
{
    public class SnackbarService
    {
        public event Func<SnackbarMessage, Task>? OnShow;

        public Task Show(string message, SnackbarType type = SnackbarType.Info, int duration = 4000)
        {
            return OnShow?.Invoke(new SnackbarMessage
            {
                Message = message,
                Type = type,
                Duration = duration
            }) ?? Task.CompletedTask;
        }
    }
}
