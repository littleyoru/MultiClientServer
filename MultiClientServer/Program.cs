using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace MultiClientServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new MyServer();

        }


    }

    class MyServer
    {
        List<TcpClient> clients = new List<TcpClient>();
        List<Socket> sockets = new List<Socket>();

        public MyServer()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 13999;
            TcpListener listener = new TcpListener(ip, port);
            listener.Start();
            AcceptClients(listener);

            bool isRunning = true;
            while (isRunning)
            {
                // Send a message
                Console.WriteLine("Server message: ");
                string text = Console.ReadLine();
                byte[] buffer = Encoding.UTF8.GetBytes(text);

                // Call encryption method
                var encryption = new EncryptionV();
                var encryptedMessage = encryption.EncryptV(buffer);
                Console.WriteLine("encrypted bytes array:");
                foreach(var b in encryptedMessage)
                    Console.Write(b);
                Console.WriteLine("\n");

                //// Call decryption method
                //var decryptedMessage = encryption.DecryptV(encryptedMessage);
                //Console.WriteLine("decrypted text is: " + decryptedMessage);


                try
                {
                    foreach (var client in clients)
                    {
                        if(client.Connected)
                            client.GetStream().Write(encryptedMessage, 0, encryptedMessage.Length);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("client not available" + ex.Message);
                }

            }
        }

        public async void ReceiveMessages(NetworkStream stream)
        {
            try
            {
                byte[] buffer = new byte[256];
                bool isRunning = true;

                while (isRunning)
                {
                    int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string text = Encoding.UTF8.GetString(buffer, 0, read);
                    Console.WriteLine("Client writes: " + text);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error when reading stream or client disconnected: " + ex.Message);
            }

        }

        public async void AcceptClients(TcpListener listener)
        {
            bool isRunning = true;
            while (isRunning)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                clients.Add(client);
                sockets.Add(client.Client);

                // Check connected clients
                Socket.Select(sockets, sockets, sockets, 1000);
                Console.WriteLine("Clients connected: ");
                foreach (var sc in sockets)
                {
                    Console.WriteLine(sc.Available);
                }

                NetworkStream stream = client.GetStream();
                ReceiveMessages(stream);

            }
        }
    }
}
