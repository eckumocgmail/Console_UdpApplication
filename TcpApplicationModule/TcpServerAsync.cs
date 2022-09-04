using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// State object for reading client data asynchronously  
public class StateObject
{
    // Size of receive buffer.  
    public const int BufferSize = 1024;

    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];

    // Received data string.
    public StringBuilder sb = new StringBuilder();

    // Client socket.
    public Socket workSocket = null;
}

public class TcpListener
{
    // Thread signal.  
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public TcpListener()
    {
    }

    public TcpListener(string ip4, int port)
    {
        this.ip4 = ip4;
        this.port = port;
    }

    private Action<string> OnMessage;
    private string v1;
    private int v2;
    private readonly string ip4;
    private readonly int port;

    public static TcpListener StartListening(Action<string> OnMessage)
    {
        // Establish the local endpoint for the socket.  
        // The DNS name of the computer  
        // running the listener is "host.contoso.com".  

        var listener = new TcpListener("127.0.0.1",11000);
        listener.Launch(OnMessage);
        
        
        return listener;
    }

    public void Launch(Action<string> OnMessage)
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = IPAddress.Parse(ip4);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
        this.OnMessage = OnMessage;
        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);
        Action<IAsyncResult> on = AcceptCallback;

        // Bind the socket to the local endpoint and listen for incoming connections.  
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(100);

            while (true)
            {
                // Set the event to nonsignaled state.  
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.  
                Console.WriteLine("Waiting for a connection...");
                listener.BeginAccept(
                    new AsyncCallback(on),
                    listener);

                // Wait until a connection is made before continuing.  
                allDone.WaitOne();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

    }

    public void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  
        allDone.Set();

        // Get the socket that handles the client request.  
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);
    }

    public void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket.
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.
            string message = Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead);
            state.sb.Append(message);
            this.OnMessage(message);

            // Check for end-of-file tag. If it is not there, read
            // more data.  
            content = state.sb.ToString();
            if (content.IndexOf("<EOF>") > -1)
            {
                // All the data has been read from the
                // client. Display it on the console.  
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    content.Length, content);
                // Echo the data back to the client.  
                Send(handler, content);
            }
            else
            {
                // Not all data received. Get more.  
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            }
        }
    }

    private static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static int Start( Action<string> listener )
    {
        StartListening(listener);
        return 0;
    }
}