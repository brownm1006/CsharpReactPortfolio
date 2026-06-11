using Npgsql;

namespace PortfolioClubAssurance.Api.Data;

public interface INpgsqlConnectionFactory
{
    Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}
