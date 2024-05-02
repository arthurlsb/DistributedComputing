using Lib;
using Newtonsoft.Json;
using Shared;

Node replica0 = new Node("config_replica0.txt");
Console.WriteLine("\nréplica 0 iniciada.");

Console.WriteLine("\nDigite a ordem de funcionamento desejada:\n1- Causal\n2- Total");
string inputOrder = Console.ReadLine();
int optionOrder = int.Parse(inputOrder);

Console.WriteLine("\nRÉPLICA 0 AGUARDANDO ADIÇÃO DE EVENTOS...\n");

if (optionOrder == 1)
{
    List<EventMessage> events = new List<EventMessage>();
    while (true)
    {
        string serializedEvent = "";
        replica0.Receive(ref serializedEvent);

        EventMessage eventReceived = new EventMessage();
        eventReceived = JsonConvert.DeserializeObject<EventMessage>(serializedEvent);

        events.Add(eventReceived);
        Console.WriteLine($"\nEVENTO: {eventReceived.EventName} DATA: {eventReceived.EventDate} adicionado ao calendário!");

        Console.WriteLine("\nLista de eventos da réplica 0: ");
        foreach (EventMessage eventsAdded in events)
        {
            Console.WriteLine($"Cliente {eventsAdded.IdClient - 3} - EVENTO: {eventsAdded.EventName} DATA: {eventsAdded.EventDate}");
        }
        Console.WriteLine("***************************************************\n");

        if (eventReceived.IsFromClient)
        {
            string serializedObject;

            serializedObject = JsonConvert.SerializeObject(eventReceived);
            replica0.Send(eventReceived.IdClient, serializedObject);

            eventReceived.IsFromClient = false;
            serializedObject = JsonConvert.SerializeObject(eventReceived);

            for (int i = 0; i < 3; i++)
            {
                if (i != 0)
                {
                    replica0.Send(i, serializedObject);
                }
            }
        }

        
    }
}
else
{
    List<EventMessage> events = new List<EventMessage>();
    while (true)
    {
        string serializedEvent = "";
        replica0.Deliver(ref serializedEvent);

        EventMessage eventReceived = new EventMessage();
        eventReceived = JsonConvert.DeserializeObject<EventMessage>(serializedEvent);

        events.Add(eventReceived);
        Console.WriteLine($"\nEVENTO: {eventReceived.EventName} DATA: {eventReceived.EventDate} adicionado ao calendário!");

        Console.WriteLine("\nLista de eventos da réplica 0: ");
        foreach (EventMessage eventsAdded in events)
        {
            Console.WriteLine($"Cliente {eventsAdded.IdClient - 3} - EVENTO: {eventsAdded.EventName} DATA: {eventsAdded.EventDate}");
        }
        Console.WriteLine("***************************************************\n");
        
        string serializedObject;

        eventReceived.IsFromClient = false;

        serializedObject = JsonConvert.SerializeObject(eventReceived);
        replica0.Send(eventReceived.IdClient, serializedObject);
    }
}