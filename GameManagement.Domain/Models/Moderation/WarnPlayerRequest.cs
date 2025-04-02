public class WarnPlayerRequest
{
    public Guid PlayerId { get; set; }
    public string Reason { get; set; }

    public void Validate()
    {
        if (PlayerId == Guid.Empty)
            throw new ArgumentException("PlayerId is required");

        if (string.IsNullOrWhiteSpace(Reason))
            throw new ArgumentException("Reason is required");
    }
}