public class MutePlayerRequest
{
    public Guid PlayerId { get; set; }
    public TimeSpan Duration { get; set; }
    public double DurationMinutes { get; set; }
    public string Reason { get; set; }

    public void Validate()
    {
        if (PlayerId == Guid.Empty)
            throw new ArgumentException("PlayerId is required");

        if (Duration <= TimeSpan.Zero || Duration > TimeSpan.FromDays(7))
            throw new ArgumentException("Duration must be between 1 second and 7 days");
    }
}