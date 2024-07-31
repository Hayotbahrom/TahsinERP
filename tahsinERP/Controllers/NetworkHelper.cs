using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

public static class NetworkHelper
{
    [DllImport("Iphlpapi.dll")]
    private static extern int SendARP(int dest, int host, ref long mac, ref int length);

    [DllImport("Ws2_32.dll")]
    private static extern int inet_addr(string ip);

    public static string GetIpAddress()
    {
        string ip = string.Empty;
        IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress[] addr = ipEntry.AddressList;

        // Assuming IPv4 address is needed
        ip = addr.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();

        return ip;
    }

    public static string GetMacAddress(string ipAddress)
    {
        int ldest = inet_addr(ipAddress);
        long macInfo = 0;
        int len = 6;
        int result = SendARP(ldest, 0, ref macInfo, ref len);

        if (result != 0)
        {
            throw new InvalidOperationException("SendARP failed.");
        }

        string macSrc = macInfo.ToString("X");
        while (macSrc.Length < 12)
        {
            macSrc = macSrc.Insert(0, "0");
        }

        string macAddress = string.Join(":", Enumerable.Range(0, 12).Where(x => x % 2 == 0).Select(x => macSrc.Substring(x, 2)));
        return macAddress;
    }
}
