namespace Stanmore.Distributor;

public class ServiceBusOptions
{
    public required string ConnectionString { get; set; }
    public required string DeleteJourneys { get; set; }
}
