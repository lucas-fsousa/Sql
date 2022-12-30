using Npgsql;
using PublicUtility.Extension;
using System.Collections;
using System.Data;

namespace PublicUtility.Sql.Postgresql {
  public partial class DB {

    #region PRIVATE METHODS

    private async Task RollBackAsync(CancellationToken cancellationToken = default) {
      if(cmd.Transaction != null)
        await cmd.Transaction.RollbackAsync(cancellationToken);
    }

    private async Task CommitAsync(CancellationToken cancellationToken = default) {
      if(cmd.Transaction != null)
        await cmd.Transaction.CommitAsync(cancellationToken);
    }

    private async ValueTask<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken = default) {
      if(con.State == ConnectionState.Closed)
        await con.OpenAsync(cancellationToken);
      
      return con;
    }

    private async Task CloseAsync() {
      if(con.State == ConnectionState.Open)
        await con.CloseAsync();
    }

    #endregion

    public async static ValueTask<DB> GetConnAsync(string connectionString, CancellationToken cancellationToken = default) => await Task.Run(DB() => new(connectionString), cancellationToken);

    public async ValueTask<string> ExecCmdAsync(NpgsqlCommand command, CancellationToken cancellationToken = default) {
      return await Task.Run(async () => {
        cmd = command;
        try {
          cmd.Connection = await OpenAsync(cancellationToken);
          cmd.Transaction = await con.BeginTransactionAsync(cancellationToken);
          await cmd.ExecuteNonQueryAsync(cancellationToken);
          await CommitAsync(cancellationToken);
          return string.Format($" [OK] - {DateTime.UtcNow}");
        } catch(Exception ex) {
          await RollBackAsync(cancellationToken);

          return string.Format($" [ERRO] - {DateTime.UtcNow} # {ex.Message} ");
        }
      }, cancellationToken);
      
    }

    public async ValueTask<string> ExecQueryAsync(string query, CancellationToken cancellationToken = default) => await ExecCmdAsync(new NpgsqlCommand(query), cancellationToken);

    public async ValueTask<DataTable> ReturnDataAsync(NpgsqlCommand command, CancellationToken cancellationToken = default) {
      return await Task.Run(async Task<DataTable>() => {
        var table = new DataTable();
        cmd = command;
        try {
          var adapter = new NpgsqlDataAdapter();
          cmd.Connection = await OpenAsync(cancellationToken);

          adapter.SelectCommand = cmd;
          adapter.Fill(table);
        } catch {
          table = null;
        }
        return table;
      }, cancellationToken);
     
    }

    public async ValueTask<DataTable> ReturnDataAsync(string query, CancellationToken cancellationToken = default) => await ReturnDataAsync(new NpgsqlCommand(query), cancellationToken);

    public async ValueTask<T> ReturnDataAsync<T>(string query, CancellationToken cancellationToken = default) where T : IEnumerable {
      var result = await ReturnDataAsync(query, cancellationToken);
      return await result.DeserializeTableAsync<T>(cancellationToken);
    }

    public async ValueTask<T> ReturnDataAsync<T>(NpgsqlCommand command, CancellationToken cancellationToken = default) where T : IEnumerable {
      var result = await ReturnDataAsync(command, cancellationToken);
      return await result.DeserializeTableAsync<T>(cancellationToken);
    }

    public async Task DisposeAsync() {
      await CloseAsync();
      GC.Collect();
    }
  }
}
