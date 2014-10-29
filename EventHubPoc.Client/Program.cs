using EventHubPoc.Common;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubPoc.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string deviceIdName = "HomeDevice-{0}";

            Console.WriteLine("Press ENTER to create clients and send events.");
            Console.ReadLine();

            var cts = new CancellationTokenSource();

            for (int i = 0; i <= EventHubSettings.PartitionCount - 1; i++)
            {
                Task.Factory.StartNew(state =>
                {
                    Console.WriteLine("Starting client partition {0}...", state);

                    MessagingFactory factory =
                        MessagingFactory.Create(
                            ServiceBusEnvironment.CreateServiceUri("sb", EventHubSettings.ServiceNamespace, ""),
                            new MessagingFactorySettings
                            {
                                TokenProvider =
                                    TokenProvider.CreateSharedAccessSignatureTokenProvider(EventHubSettings.ClientKeyName, EventHubSettings.ClientSharedAccessKey),
                                TransportType = TransportType.Amqp
                            });

                    while (true)
                    {
                        var eventBody = new TemperatureEvent
                        {
                            DeviceId = string.Format(deviceIdName, state),
                            Temperature = new Random().Next(20, 50)
                        };

                        EventHubClient client =
                            factory.CreateEventHubClient(String.Format("{0}/publishers/{1}", EventHubSettings.HubName, state));

                        var message = new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventBody)))
                        {
                            PartitionKey = state.ToString()
                        };

                        client.Send(message);

                        Console.WriteLine("{0}: Partition [{1}] - Temperature: {2}", DateTime.Now, message.PartitionKey,
                                    eventBody.Temperature);

                        Thread.Sleep(new Random().Next(50, 60));

                        if (!cts.IsCancellationRequested) continue;

                        Console.WriteLine("Stopping Client Partition {0}...", state);
                        client.Close();
                        break;
                    }
                }, i, cts.Token);
            }

            Console.ReadLine();
            cts.Cancel();
            Console.WriteLine("Wait for all senders to close and then press ENTER.");
            Console.ReadLine();
        }
    }
}