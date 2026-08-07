using Npgsql;
using System.Data.Common;

namespace IrrigationApi.Data
{
    public interface IDbConnectionFactory
    {
        Task<DbConnection> CreateAsync(CancellationToken cancellationToken);
    }

    public class NpgsqlDataSourceFactory : IDbConnectionFactory, IAsyncDisposable
    {
        private readonly NpgsqlDataSource _myDataSource;

        public NpgsqlDataSourceFactory(string connectionString)
        {
            _myDataSource = NpgsqlDataSource.Create(connectionString);
        }

        public async Task<DbConnection> CreateAsync(CancellationToken cancellationToken)
        {
            return await _myDataSource.OpenConnectionAsync(cancellationToken);
        }

        // Implement IAsyncDisposable
        public ValueTask DisposeAsync()
        {
            return _myDataSource.DisposeAsync();
        }
    }
}