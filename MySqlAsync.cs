using MySqlConnector;
using PublicUtility.Extension;
using System.Collections;
using System.Data;

namespace PublicUtility.Sql.MySql {
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

    private async ValueTask<MySqlConnection> OpenAsync(CancellationToken cancellationToken = default) {
      if(con.State == ConnectionState.Closed) {
        await con.OpenAsync(cancellationToken);
        tran = await con.BeginTransactionAsync(cancellationToken);
      }
      return con;
    }

    private async Task CloseAsync() {
      if(con.State == ConnectionState.Open)
        await con.CloseAsync();
    }

    #endregion

    public async static ValueTask<DB> GetConnAsync(MySqlConnectionStringBuilder connectionBuilder, CancellationToken cancellationToken = default) => await Task.Run(DB () => new(connectionBuilder), cancellationToken);

    public async ValueTask<string> ExecCmdAsync(MySqlCommand command, CancellationToken cancellationToken = default) {
      return await Task.Run(async () => {
        cmd = command;
        try {
          cmd.Connection = await OpenAsync(cancellationToken);
          cmd.Transaction = tran;
          await cmd.ExecuteNonQueryAsync(cancellationToken);
          await CommitAsync(cancellationToken);
          return string.Format($" [OK] - {DateTime.UtcNow}");
        } catch(Exception ex) {
          await RollBackAsync(cancellationToken);

          return string.Format($" [ERRO] - {DateTime.UtcNow} # {ex.Message} ");
        }
      }, cancellationToken);
      
    }

    public async ValueTask<string> ExecQueryAsync(string query, CancellationToken cancellationToken = default) => await ExecCmdAsync(new MySqlCommand(query), cancellationToken);

    public async ValueTask<DataTable> ReturnDataAsync(MySqlCommand command, CancellationToken cancellationToken = default) {
      return await Task.Run(async Task<DataTable> () => {
        var table = new DataTable();
        cmd = command;
        try {
          var adapter = new MySqlDataAdapter();
          cmd.Connection = await OpenAsync(cancellationToken);
          cmd.Transaction = tran;

          adapter.SelectCommand = cmd;
          adapter.Fill(table);
          await CommitAsync(cancellationToken);
        } catch {
          table = null;
          await RollBackAsync(cancellationToken);
        }
        return table;
      }, cancellationToken); 
    }

    public async ValueTask<DataTable> ReturnDataAsync(string query, CancellationToken cancellationToken = default) => await ReturnDataAsync(new MySqlCommand(query), cancellationToken);

    public async ValueTask<T> ReturnDataAsync<T>(string query, CancellationToken cancellationToken = default) where T : IEnumerable {
      var response = await ReturnDataAsync(query, cancellationToken);
      return await response.DeserializeTableAsync<T>(cancellationToken);
    }

    public async ValueTask<T> ReturnDataAsync<T>(MySqlCommand command, CancellationToken cancellationToken = default) where T : IEnumerable {
      var response = await ReturnDataAsync(command, cancellationToken);
      return await response.DeserializeTableAsync<T>(cancellationToken);
    }

    public async Task DisposeAsync() {
      await CloseAsync();
      GC.Collect();
    }
  }
}
