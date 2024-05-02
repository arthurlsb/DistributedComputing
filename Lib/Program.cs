using Lib;

Node replica0 = new Node("config_replica0.txt");
Node replica1 = new Node("config_replica1.txt");
Node replica2 = new Node("config_replica2.txt");

Console.WriteLine("Digite a ordem que deseja testar:\n1- Causal\n2- Total");
string input = Console.ReadLine();
int option = int.Parse(input);

string inputNumMsgs;
int numMsgs;

if (option == 1)
{
    Console.WriteLine("Digite a quantidade de mensagens que deseja enviar/receber:");
    inputNumMsgs = Console.ReadLine();
    numMsgs = int.Parse(inputNumMsgs); 

    for (int i = 0; i < numMsgs; i++)
    {
        replica0.Send(1, $"{i}");
    }

    for (int i = 0; i < numMsgs; i++)
    {
        replica1.Send(2, $"{i}");
    }

    for (int i = 0; i < numMsgs; i++)
    {
        string message = "";
        replica1.Receive(ref message);
        Console.WriteLine($"réplica 1 recebeu: {message}");
    }

    for (int i = 0; i < numMsgs; i++)
    {
        string message = "";
        replica2.Receive(ref message);
        Console.WriteLine($"réplica 2 recebeu: {message}");
    }
}

else
{
    Console.WriteLine("Digite a quantidade de mensagens que deseja fazer o broadcast/fazer o delivery:");
    inputNumMsgs = Console.ReadLine();
    numMsgs = int.Parse(inputNumMsgs);

    for (int i = 0; i < numMsgs; i++)
    {
        replica0.Broadcast($"{i}");
    }

    for (int i = 0; i < numMsgs; i++)
    {
        string message = "";
        replica0.Deliver(ref message);
        Console.WriteLine($"réplica 0 fez o delivery de: {message}");
    }

    for (int i = 0; i < numMsgs; i++)
    {
        string message = "";
        replica1.Deliver(ref message);
        Console.WriteLine($"réplica 1 fez o delivery de: {message}");
    }

    for (int i = 0; i < numMsgs; i++)
    {
        string message = "";
        replica2.Deliver(ref message);
        Console.WriteLine($"réplica 2 fez o delivery de: {message}");
    }
}

Console.WriteLine("TESTES FINALIZADOS!");





