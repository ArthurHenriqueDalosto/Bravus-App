namespace BravusApp.Client.Model
{
    public class DayInfo
    {
        public int DayNumber { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool HasSV { get; set; }
        public int CountDuties { get; set; }
        public bool IsFull { get; set; }

        public DateOnly? Date => DayNumber > 0 ? new DateOnly(Year, Month, DayNumber) : null;
    }
}
