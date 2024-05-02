namespace Lib;
public class Message
{
    public int IdSender { get; set; }
    public int IdReceiver { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Content { get; private set; }
    public VectorClock VectorClock { get; set; }
    public int sequenceNumber;

    public Message(int idSender, int idReceiver, string content)
    {
        IdSender = idSender;
        IdReceiver = idReceiver;
        CreatedAt = DateTime.Now;
        Content = content;
        VectorClock = new VectorClock();
        sequenceNumber = 0; 
    }
}
