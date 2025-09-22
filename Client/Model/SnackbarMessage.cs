namespace BravusApp.Client.Model
{
    public class SnackbarMessage
    {
        public string Message { get; set; } = string.Empty;
        public SnackbarType Type { get; set; }
        public int Duration { get; set; }
    }
}
