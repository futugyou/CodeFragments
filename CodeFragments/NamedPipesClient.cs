using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;

namespace CodeFragments;
public class NamedPipesClient
{
    private static int numClients = 4;
    public static void Base()
    {
        var pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

        Console.WriteLine("Connecting to server...\n");
        pipeClient.Connect();

        var ss = new StreamString(pipeClient);
        // Validate the server's signature string.
        if (ss.ReadString() == "I am the one true server!")
        {
            // The client security token is sent with the first write.
            // Send the name of the file whose contents are returned
            // by the server.
            ss.WriteString("NamedPipesClient.cs");

            // Print the file to the screen.
            Console.Write(ss.ReadString());
        }
        else
        {
            Console.WriteLine("Server could not be verified.");
        }
        pipeClient.Close();
        // Give the client process some time to display results before exiting.
        Thread.Sleep(4000);
    }
}
