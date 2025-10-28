using Org.BouncyCastle.Tls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using UnitTesting;
using static System.Net.Mime.MediaTypeNames;


internal class Program 
{
    public static async Task MainAsync() 
    {
        ListenerDBTests listenerDBTests = new ListenerDBTests();
        await listenerDBTests.RunAllTests();
    }

    public static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }
}