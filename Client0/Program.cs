using Lib;
using Newtonsoft.Json;
using Shared;
using System.Globalization;

Node client0 = new Node("config_client0.txt");
Console.WriteLine("\nClient 0 iniciado.");

Console.WriteLine("\nDigite a ordem de funcionamento desejada:\n1- Causal\n2- Total");
string inputOrder = Console.ReadLine();
int optionOrder = int.Parse(inputOrder);

int numOfProcesses = 3;

if (optionOrder == 1)
{
    List<EventMessage> events = new List<EventMessage>();
    while (true)
    {
        Console.WriteLine("\nEscolha uma opção:");
        Console.WriteLine("1- Adicionar novo evento ao calendário");
        Console.WriteLine("2- Listar eventos do calendário");
        string inputAction = Console.ReadLine();
        int optionAction = int.Parse(inputAction);

        if (optionAction == 1)
        {
            Console.WriteLine("\nDigite o nome do evento:");
            string inputEventName = Console.ReadLine();

            typeDate:
            Console.WriteLine("\nDigite a data no formato dd/mm/yyyy HH:mm:");
            string inputDate = Console.ReadLine();

            DateTime dateTime;

            try
            {
                dateTime = DateTime.ParseExact(inputDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                Console.WriteLine("\nDATA DIGITADA NO FORMATO ERRADO!");
                goto typeDate;
            }
            
            EventMessage eventMessage = new EventMessage(); 
            eventMessage.Id = Guid.NewGuid();
            eventMessage.IdClient = client0.id;
            eventMessage.EventName = inputEventName;
            eventMessage.EventDate = dateTime;
            eventMessage.IsFromClient = true;
            
            string serializedObject = JsonConvert.SerializeObject(eventMessage);

            client0.Send(1, serializedObject);

            string serializedEvent = "";
            client0.Receive(ref serializedEvent);

            EventMessage eventReceived = new EventMessage();
            eventReceived = JsonConvert.DeserializeObject<EventMessage>(serializedEvent);
            
            events.Add(eventReceived);
            Console.WriteLine($"\nEVENTO: {eventReceived.EventName} DATA: {eventReceived.EventDate} adicionado ao calendário!");
        }

        if (optionAction == 2)
        {
            foreach (EventMessage eventsAdded in events)
            {
                Console.WriteLine($"EVENTO: {eventsAdded.EventName} DATA: {eventsAdded.EventDate}");
            }
        }
    }

}
else
{
    List<EventMessage> events = new List<EventMessage>();
    while (true)
    {
        Console.WriteLine("\nEscolha uma opção:");
        Console.WriteLine("1- Adicionar novo evento ao calendário");
        Console.WriteLine("2- Listar eventos do calendário");
        string inputAction = Console.ReadLine();
        int optionAction = int.Parse(inputAction);

        if (optionAction == 1)
        {
            Console.WriteLine("\nDigite o nome do evento:");
            string inputEventName = Console.ReadLine();

            typeDate:
            Console.WriteLine("\nDigite a data no formato dd/mm/yyyy HH:mm:");
            string inputDate = Console.ReadLine();

            DateTime dateTime;

            try
            {
                dateTime = DateTime.ParseExact(inputDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                Console.WriteLine("\nDATA DIGITADA NO FORMATO ERRADO!");
                goto typeDate;
            }
            
            EventMessage eventMessage = new EventMessage();
            eventMessage.Id = Guid.NewGuid();
            eventMessage.IdClient = client0.id;
            eventMessage.EventName = inputEventName;
            eventMessage.EventDate = dateTime;
            eventMessage.IsFromClient = true;

            string serializedObject = JsonConvert.SerializeObject(eventMessage);

            client0.Broadcast(serializedObject);

            string serializedEvent = "";

            for (int i = 0; i < numOfProcesses; i++)
            {
                client0.Receive(ref serializedEvent);

                EventMessage eventReceived = new EventMessage();
                eventReceived = JsonConvert.DeserializeObject<EventMessage>(serializedEvent);

                if (!events.Any(obj => obj.Id == eventReceived.Id))
                {
                    events.Add(eventReceived);
                    Console.WriteLine($"\nEVENTO: {eventReceived.EventName} DATA: {eventReceived.EventDate} adicionado ao calendário!");
                }
            }
        }

        if (optionAction == 2)
        {
            foreach (EventMessage eventsAdded in events)
            {
                Console.WriteLine($"EVENTO: {eventsAdded.EventName} DATA: {eventsAdded.EventDate}");
            }
        }
    }

}