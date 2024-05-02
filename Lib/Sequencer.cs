using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Lib;
public class Sequencer
{
    public Address Address { get; set; }
    public Address[] Addresses { get; private set; }

    private Thread sequencerThread;

    List<Message> receivedMessages = new List<Message>();

    int sequenceNumber = 0;
  
    public Sequencer() {
        Address = new Address("127.0.0.1", 5999);

        Addresses = new Address[]
        {
            new Address("127.0.0.1", 6000),
            new Address("127.0.0.1", 6001),
            new Address("127.0.0.1", 6002)
        };

        sequencerThread = new Thread(new ThreadStart(runSequencerSocket));
        sequencerThread.Start();
    }

    public void runSequencerSocket()
    {
        Socket sequencerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 5999;
            sequencerSocket.Bind(new IPEndPoint(ip, port));
            sequencerSocket.Listen(10);

            while (true)
            {
                Socket acceptedSocket = sequencerSocket.Accept();
                byte[] buffer = new byte[1024];
                int bytesRead = acceptedSocket.Receive(buffer);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Message messageReceived = JsonConvert.DeserializeObject<Message>(message);

                messageReceived.sequenceNumber = sequenceNumber;
                sequenceNumber++;
                   
                receivedMessages.Add(messageReceived);
                SendToAll(messageReceived);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro no socket: " + ex.Message);
        }
        finally
        {
            sequencerSocket.Close();
        }
    }

    public void SendToAll(Message message)
    {
        // TODO: Verificar se precisa mandar pra ele mesmo
        for (int i = 0; i < Addresses.Length; i++)
        {
            Message messageToSent = new Message(-1, i, message.Content);
            messageToSent.CreatedAt = message.CreatedAt;
            messageToSent.sequenceNumber = message.sequenceNumber;

            Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string serverIP = "127.0.0.1";
            int serverPort = 6000 + i;

            senderSocket.Connect(serverIP, serverPort);
            string serializedMessage = JsonConvert.SerializeObject(messageToSent);
            byte[] buffer = Encoding.ASCII.GetBytes(serializedMessage);
            senderSocket.Send(buffer);
        }
    }
}
