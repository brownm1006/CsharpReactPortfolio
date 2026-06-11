using Microsoft.Extensions.Options;
using Npgsql;
using PortfolioClubAssurance.Api.Options;

namespace PortfolioClubAssurance.Api.Data;

public sealed class NpgsqlConnectionFactory : INpgsqlConnectionFactory
{
    private readonly QuoteDatabaseOptions options;

    public NpgsqlConnectionFactory(IOptions<QuoteDatabaseOptions> options)
    {
        this.options = options.Value;
    }

    public async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.QuoteDatabase))
        {
            throw new InvalidOperationException("Missing ConnectionStrings:QuoteDatabase configuration.");
        }

        var connection = new NpgsqlConnection(options.QuoteDatabase);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
