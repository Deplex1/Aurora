using Org.BouncyCastle.Tls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using UnitTesting;
using static System.Net.Mime.MediaTypeNames;


public class Program 
{
    public static void Main(string[] args) 
    {
        ListenerDBTests listenerDBTests = new ListenerDBTests();
        listenerDBTests.RunAllTests();
    }
}