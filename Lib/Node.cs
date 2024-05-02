using System.Net.Sockets;
using System.Text;
using System.Net;
using Newtonsoft.Json;

namespace Lib;
public class Node
{
    public int id;
    private string nodeIp = "127.0.0.1";
    VectorClock vectorClock = new();
    List<Message> receivedMessages = new();
    List<Message> pendingMessages = new();
    List<Message> messagesAlreadyReceiveds = new();
    List<Message> temporaryListCausal = new();
    List<Message> temporaryListTotal = new();
    private Thread receiverThread;
    private Thread causalOrderDelayThread;
    private Thread totalOrderDelayThread;
    static Mutex mutex = new();
    static Mutex mutex2 = new();
    public int nextDeliver;
    Message messageToReceive;
    Message messageToDeliver;

    public Node(string fileToRead)
    {
        Init(fileToRead);
    }
    
    public void Init(string file)
    {
        string exePath = AppDomain.CurrentDomain.BaseDirectory;
        string configFolderPath = Path.Combine(exePath, "Configs");
        string configFile = Path.Combine(configFolderPath, file);

        int numProcesses, idValue;
        ReadConfigFile(configFile, out numProcesses, out idValue);
        id = idValue;

        if (id == 0)
        {
            Sequencer sequencer = new Sequencer();
        }

        receiverThread = new Thread(new ThreadStart(runReceiverSocket));
        receiverThread.Start();
    }

    public void runReceiverSocket()
    {
        Socket receiverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            IPAddress ip = IPAddress.Parse(nodeIp);
            int port = 6000 + id;
            receiverSocket.Bind(new IPEndPoint(ip, port));
            receiverSocket.Listen(1000);

            while (true)
            {
                Socket acceptedSocket = receiverSocket.Accept();
                byte[] buffer = new byte[1024];
                int bytesRead = acceptedSocket.Receive(buffer);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Message messageReceived = JsonConvert.DeserializeObject<Message>(message);
                
                if (messageReceived.IdSender == -1)
                {
                    totalOrderDelayThread = new Thread(() => DelayTotalOrder(messageReceived));
                    totalOrderDelayThread.Start();
                }
                else
                {
                    causalOrderDelayThread = new Thread(() => DelayCausalOrder(messageReceived));
                    causalOrderDelayThread.Start();
                }

                acceptedSocket.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro no socket: " + ex.Message);
        }
        finally
        {
            receiverSocket.Close();
        }
    }

    public void DelayCausalOrder(Message messageReceived)
    {
        Random random = new Random();
        Thread.Sleep(random.Next(0, 4000));
        mutex.WaitOne();
        
        if (id != 3 && id != 4)
        {
            Console.WriteLine($"Chegou na réplica {id}: {messageReceived.Content}");
        }
        
        receivedMessages.Add(messageReceived);
        mutex.ReleaseMutex();
        
    }

    public void DelayTotalOrder(Message messageReceived)
    {
        Random random = new Random();
        Thread.Sleep(random.Next(0, 4000));
        mutex2.WaitOne();
        Console.WriteLine($"Chegou na réplica {id}: {messageReceived.Content}");
        pendingMessages.Add(messageReceived);
        mutex2.ReleaseMutex();
    }

    public void Send(int idReceiver, string message)
    {
        Message messageToSent = new Message(id, idReceiver, message);
        messageToSent.VectorClock = vectorClock;

        Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string serverIP = nodeIp;
        int serverPort = 6000 + idReceiver;

        senderSocket.Connect(serverIP, serverPort);
        string serializedMessage = JsonConvert.SerializeObject(messageToSent);
        byte[] buffer = Encoding.ASCII.GetBytes(serializedMessage);
        senderSocket.Send(buffer);

        vectorClock.IncrementSenti(id, idReceiver);

        senderSocket.Close();
    }

    public void Receive(ref string messageToReturn)
    {
        int aux = 0;
        while (true)
        {
            temporaryListCausal.AddRange(receivedMessages);

            foreach (Message message in temporaryListCausal)
            {
                if (message != null)
                {
                    if (vectorClock.IsCausal(message.VectorClock.Senti, message.IdReceiver))
                    {
                        if (!CheckContains(messagesAlreadyReceiveds, message))
                        {
                            //Console.WriteLine($"Para todo k->    [{this.vectorClock.Delivi[0]}, {this.vectorClock.Delivi[1]}, {this.vectorClock.Delivi[2]}] >= [{message.VectorClock.Senti[0, this.id]}\n\t\t\t\t{message.VectorClock.Senti[1, this.id]}\n\t\t\t\t{message.VectorClock.Senti[2, this.id]}]");
                            vectorClock.Update(message.VectorClock.Senti, id, message.IdSender);
                            messagesAlreadyReceiveds.Add(message);
                            messageToReceive = new Message(message.IdSender, message.IdReceiver, message.Content);
                            aux = 1;
                            break;
                        }
                    } 
                }
            }

            temporaryListCausal.Clear();

            if (aux == 1)
            {
                break;
            }
        }

        messageToReturn = messageToReceive.Content;
    }

    public void Broadcast(string message)
    {
        Message messageToSent = new Message(id, -1, message);
        Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string serverIP = "127.0.0.1";
        int serverPort = 5999;

        senderSocket.Connect(serverIP, serverPort);
        string serializedMessage = JsonConvert.SerializeObject(messageToSent);
        byte[] buffer = Encoding.ASCII.GetBytes(serializedMessage);
        senderSocket.Send(buffer);
    }

    public void Deliver(ref string messageToReturn)
    {
        int aux = 0;
        while (true)
        {
            temporaryListTotal.AddRange(pendingMessages);

            foreach (Message message in temporaryListTotal)
            {
                if (message != null)
                {
                    if (message.sequenceNumber == nextDeliver)
                    {
                        if (!CheckContains(messagesAlreadyReceiveds, message))
                        {
                            nextDeliver++;
                            messagesAlreadyReceiveds.Add(message);
                            messageToDeliver = new Message(message.IdSender, message.IdReceiver, message.Content);
                            aux = 1;
                            break;
                        }
                
                    }
                }
            }

            temporaryListTotal.Clear();

            if (aux == 1)
            {
                break;
            }
        }

        messageToReturn = messageToDeliver.Content;
    }

    public static bool CheckContains(List<Message> messageList, Message message2)
    {
        return messageList.Any(obj => obj.Content == message2.Content && obj.CreatedAt == message2.CreatedAt && obj.IdSender == message2.IdSender);
    }

    static void ReadConfigFile(string filePath, out int numProcesses, out int idValue)
    {
        numProcesses = -1; 
        idValue = -1; 

        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("processes"))
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            if (int.TryParse(parts[1].Trim(), out numProcesses))
                            {
                                continue;
                            }
                        }
                    }

                    if (line.StartsWith("id"))
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            if (int.TryParse(parts[1].Trim(), out idValue))
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error reading the file: " + e.Message);
        }
    }

}
