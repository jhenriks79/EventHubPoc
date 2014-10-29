using EventHubPoc.Common;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubPoc.Host
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            for (int i = 0; i <= EventHubSettings.PartitionCount - 1; i++)
            {
                Task.Factory.StartNew(state =>
                {
                    Console.WriteLine("Starting host partition {0}...", state);

                    MessagingFactory factory =
                        MessagingFactory.Create(ServiceBusEnvironment.CreateServiceUri("sb", EventHubSettings.ServiceNamespace, ""),
                            new MessagingFactorySettings
                            {
                                TokenProvider =
                                    TokenProvider.CreateSharedAccessSignatureTokenProvider(EventHubSettings.HostKeyName, EventHubSettings.HostSharedAccessKey),
                                TransportType = TransportType.Amqp
                            });

                    EventHubReceiver receiver = factory.CreateEventHubClient(EventHubSettings.HubName)
                        .GetDefaultConsumerGroup()
                        .CreateReceiver(state.ToString(), DateTime.UtcNow);

                    while (true)
                    {
                        try
                        {
                            IEnumerable<EventData> messages = receiver.Receive(10);
                            foreach (EventData message in messages)
                            {
                                var eventBody =
                                    JsonConvert.DeserializeObject<TemperatureEvent>(
                                        Encoding.Default.GetString(message.GetBytes()));
                                Console.WriteLine("{0}: Partition [{1}] - Temperature: {2}", DateTime.Now, message.PartitionKey,
                                    eventBody.Temperature);
                            }

                            if (!cts.IsCancellationRequested) continue;

                            Console.WriteLine("Stopping Host Partition {0}...", state);
                            receiver.Close();
                            break;
                        }
                        catch
                        {
                            // Receive could fail, I would need a retry policy etc...
                            break;
                        }
                    }
                }, i, cts.Token);
            }

            Console.ReadLine();
            cts.Cancel();
            Console.WriteLine("Wait for all receivers to close and then press ENTER.");
            Console.ReadLine();
        }
    }
}