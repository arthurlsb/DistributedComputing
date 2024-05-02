namespace Shared;
public class EventMessage
{
    public Guid Id { get; set; }
    public int IdClient { get; set; }
    public string EventName { get; set; }
    public DateTime EventDate { get; set; }
    public bool IsFromClient { get; set; }
}
