using Chetch.ChetchXMPP;
using Chetch.Messaging;
using Chetch.Utilities;

namespace ConsoleAppChetchXMPPClient;

class Program
{
    const String USERNAME = "mactest@openfire.bb.lan";
    const String PASSWORD = "mactest";
    //const String TARGET = "bbalarms.service@openfire.bb.lan";
    const String TARGET = "bbengineroom.service@openfire.bb.lan";
    const String EXIT_COMMAND = "x";


    static ChetchXMPPConnection cnn;
    static String target = TARGET;
    
    static void ConnectClient()
    {
        try
        {
            cnn = new ChetchXMPPConnection(USERNAME, PASSWORD);
            Console.WriteLine("Connecting {0}...", USERNAME);

            cnn.ConnectAsync();
            Thread.Sleep(500);
            while (!cnn.Ready)
            {
                Console.WriteLine("Waiting to connect...");
                Thread.Sleep(1000);
            }
            Console.WriteLine("Connected!");

            cnn.MessageReceived += (sender, eargs)=>{
                var msg = eargs.Message;
                if (msg == null) return;

                String output = "";
                switch (msg.Type)
                {
                    case MessageType.ERROR:
                        String errorMessage = msg.HasValue("ErrorMessage") ? msg.GetString("ErrorMessage") : msg.GetString("Message");
                        output = String.Format("<----- !!!!ERROR!!!! {0} received from {1}", errorMessage, msg.Sender);
                        break;

                    case MessageType.STATUS_RESPONSE:
                        output = String.Format("****SERVER****: Time={0}, Offset={1} ", msg.Get<DateTime>("ServerTime"), msg.Get<int>("ServerTimeOffset"));
                        break;

                    case MessageType.COMMAND_RESPONSE:
                        output = String.Format("<----- Message {0} received from {1}", msg.Type, msg.Sender);
                        String originalCommand = msg.GetString("OriginalCommand");
                        break;

                    case MessageType.NOTIFICATION:
                        output = String.Format("<----- Message {0} received from {1}", msg.Type, msg.Sender);
                        break;

                    default:
                        output = String.Format("<----- Message {0} received from {1}", msg.Type, msg.Sender);
                        break;
                }
                Console.WriteLine();
                Console.WriteLine(output);
                Console.WriteLine();
            };

        }
        catch (Exception e)
        {
            Console.WriteLine("Connect cliente exception: {0}", e.Message);
        }
    }

    static void SendCommand(String commandAndArgs)
    {
        try
        {
            var cmd = ChetchXMPPMessaging.CreateCommandMessage(commandAndArgs);
            if(cmd.GetString(ChetchXMPPMessaging.MESSAGE_FIELD_COMMAND) == "alert")
            {
                cmd.Type = MessageType.ALERT;
                var args = cmd.GetList<String>(Chetch.ChetchXMPP.ChetchXMPPMessaging.MESSAGE_FIELD_ARGUMENTS);
                
            }

            cmd.Target = target;
            cnn.SendMessageAsync(cmd);
        }
        catch (Exception e)
        {
            Console.WriteLine("Send exceptione: {0}", e.Message);
        }
    }

    static void DisconnectClient()
    {
        cnn.DisconnectAsync();
    }


    static void Main(string[] args)
    {
        try
        {
            ConsoleHelper.PK("Press a key to connect...");
            ConnectClient();

            Console.WriteLine("Yewwww connected!");

            ConsoleHelper.PK("Press a key to end...");
        } 
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e.Message);

        }
    }
}
