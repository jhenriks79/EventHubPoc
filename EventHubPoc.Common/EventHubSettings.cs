namespace EventHubPoc.Common
{
    public class EventHubSettings
    {
        public const string ClientKeyName = ""; // Name of the Client's Shared Access Policy
        public const string ClientSharedAccessKey = ""; // Key for the Client's Shared Access Policy
        public const string HostKeyName = ""; // Name of the Host's Shared Access Policy
        public const string HostSharedAccessKey = ""; // Key for the Host's Shared Access Policy
        public const int PartitionCount = 1; // Number of threads/partitions to create.
        public const string ServiceNamespace = ""; // Service Bus Namespace
        public const string HubName = ""; // Even Hub Name
    }
}