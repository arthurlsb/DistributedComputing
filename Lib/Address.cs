namespace Lib;
public class Address
{
    public string Ip { get; set; }
    public int Port { get; set; }

    public Address(string ip, int port)
    {
        Ip = ip;
        Port = port;
    }
}
