

namespace Infrastructure.Persistence.Initialization;
internal interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken);
    Task InitializeApplicationDbAsync(CancellationToken cancellationToken);
}