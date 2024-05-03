using CommandLine;
using RestSharp;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

/***************************************************************************************************************************
 * 
 * A simple console application to call a Restful POST to activate a Sequence Trigger passing in a Bearer token.
 * 
 * Developed by: Cave Arnold
 * 
 * Version 1.0.1 : 5/1/2024 - Initial Release.
 * 
 **************************************************************************************************************************/

namespace SequenceAPITest
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string? SequenceTriggerEndpoint = "";
            string? SequenceTriggerSecret = "";
            Boolean Interactive = true;
            String UUID = Guid.NewGuid().ToString();
            String RoamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            Log.Logger = new LoggerConfiguration()
                            // add console as logging target
                            .WriteTo.Console()
                            // add a logging target for warnings and higher severity logs
                            // structured in JSON format
                            .WriteTo.File(new JsonFormatter(), RoamingAppData + "\\" + AppDomain.CurrentDomain.FriendlyName + "\\Logs\\" + AppDomain.CurrentDomain.FriendlyName + " Important .json",
                                          restrictedToMinimumLevel: LogEventLevel.Warning,
                                          rollingInterval: RollingInterval.Day)
                            // add a rolling file for all logs
                            .WriteTo.File(RoamingAppData + "\\" + AppDomain.CurrentDomain.FriendlyName + "\\Logs\\" + AppDomain.CurrentDomain.FriendlyName + " Verbose .logs",
                                          rollingInterval: RollingInterval.Day)
                            // set default minimum level
                            .MinimumLevel.Debug()
                            .CreateLogger();

            Log.Debug("Serilog Log File: {logs}", RoamingAppData + "\\" + AppDomain.CurrentDomain.FriendlyName + "\\Logs\\" + AppDomain.CurrentDomain.FriendlyName + " Verbose .logs");

            Log.Debug("Prior to parsing commandline.");
            Log.Information(AppDomain.CurrentDomain.FriendlyName);

            Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsed(options =>
             {
                SequenceTriggerEndpoint = options.Endpoint;

                SequenceTriggerSecret = options.Secret;

                Interactive = options.Interactive;
            });

            if (args.Count() >= 4)
            {

                Log.Debug("Interactive is {interactive}.", Interactive);

                Log.Debug("Prior to RestSharp POST.");

                Log.Debug("Idempotency UUID(GUID): {uuid}", UUID);

                var client = new RestClient(SequenceTriggerEndpoint);

                var request = new RestRequest();

                // Add Rest Headers
                request.AddHeader("x-sequence-signature", "Bearer " + SequenceTriggerSecret);
                request.AddHeader("idempotency-key", UUID);
                request.AddHeader("content-type", "application/json");

                // Add Rest Body
                request.AddBody("{}");

                // Catch any unhandled errors.
                try
                {

                    var response = client.Post(request);

                    // Raw content as string
                    var content = response.Content;

                }
                catch (Exception Error)
                {
                    Log.Debug("Error with RestSharp POST: {error} .", Error);
                }


                Log.Debug("After RestSharp POST.");

                if (!Interactive)
                {
                    Console.WriteLine("\n\nPress the spacebar to exit.");
                    do
                    {
                        while (!Console.KeyAvailable)
                        {
                            // Do something
                        }
                    } while (Console.ReadKey(true).Key != ConsoleKey.Spacebar);
                }
            }
        }

        // CLI options helper class
        public class CommandLineOptions
        {
            [Option('e', "endpoint", Required = true, HelpText = "Sequence Rule Trigger Endpoint.")]
            public string? Endpoint { get; set; }

            [Option('s', "secret", Required = true, HelpText = "Sequence Rule Trigger Secret.")]
            public string? Secret { get; set; }

            [Option('i', "interactive", Required = false, Default = true, HelpText = "Is this being run interactively?")]
            public Boolean Interactive { get; set; }

        }
    }
}
