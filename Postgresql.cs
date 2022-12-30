using Npgsql;
using PublicUtility.Extension;
using System.Collections;
using System.Data;

namespace PublicUtility.Sql.Postgresql {
  public partial class DB: IDisposable {
    private readonly NpgsqlConnection con = null;
    private NpgsqlCommand cmd = null;

    #region PRIVATE METHODS

    private void RollBack() {
      if(cmd.Transaction != null)
        cmd.Transaction.Rollback();
    }

    private void Commit() {
      if(cmd.Transaction != null)
        cmd.Transaction.Commit();
    }

    private NpgsqlConnection Open() {
      if(con.State == ConnectionState.Closed)
        con.Open();
      
      return con;
    }

    private void Close() {
      if(con.State == ConnectionState.Open)
        con.Close();
    }

    #endregion

    private DB(string connectionString) {
      con = new NpgsqlConnection(connectionString);
    }

    public static DB GetConn(string connectionString) => new(connectionString);

    public string ExecCmd(NpgsqlCommand command) {
      cmd = command;
      try {
        cmd.Connection = Open();
        cmd.Transaction = con.BeginTransaction();
        cmd.ExecuteNonQuery();
        Commit();
        return string.Format($" [OK] - {DateTime.UtcNow}");
      } catch(Exception ex) {
        RollBack();

        return string.Format($" [ERRO] - {DateTime.UtcNow} # {ex.Message} ");
      }
    }

    public string ExecQuery(string query) => ExecCmd(new NpgsqlCommand(query));

    public DataTable ReturnData(NpgsqlCommand command) {
      var table = new DataTable();
      cmd = command;
      try {
        var adapter = new NpgsqlDataAdapter();
        cmd.Connection = Open();

        adapter.SelectCommand = cmd;
        adapter.Fill(table);
      } catch {
        table = null;
      }
      return table;
    }

    public DataTable ReturnData(string query) => ReturnData(new NpgsqlCommand(query));

    public T ReturnData<T>(string query) where T : IEnumerable => ReturnData(query).DeserializeTable<T>();

    public T ReturnData<T>(NpgsqlCommand command) where T : IEnumerable => ReturnData(command).DeserializeTable<T>();

    public void Dispose() {
      Close();
      GC.SuppressFinalize(this);
      GC.Collect();
    }
  }
}
