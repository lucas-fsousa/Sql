# Extension

Class library with SQL manipulation facilitator in multiple database types

## Installation

To install, just run the C# compiler to generate the .dll file and once the file has been generated, just add the reference to the project or use [Nuget](https://www.nuget.org/packages/PublicUtility.Extension) or in nuget console, use the following command:


```bash
install-Package PublicUtility.Sql
```

## DB Class
Class used for work with sql server database. Simplified methods that use sql transaction to complete activities securely. Below are examples of usage.

#### How to use - SQLSERVER:
```csharp
using PublicUtility.Sql.SqlServer;

// ************* USE EXAMPLE 1 ************
using var conn = DB.GetConn("myConnString"); // opens a new Sql server connection
var query = new StringBuilder();
var cmd = new SqlCommand();

query.AppendLine("SELECT * FROM MYTABLE");
query.AppendLine("WHERE ID = @ID");

cmd.CommandText = query.ToString();
cmd.Parameters.AddWithValue("ID", 10);

var dataTable = conn.ReturnData(cmd);


// ************* USE EXAMPLE 2 ************
using var conn = DB.GetConn("myConnString"); // opens a new Sql server connection
var query = "UPDATE MY TABLE SET NAME = NEWNAME WHERE ID = @ID";
var cmd = new SqlCommand();

cmd.CommandText = query.ToString();
cmd.Parameters.AddWithValue("ID", 10);
cmd.Parameters.AddWithValue("NEWNAME", "IS A TEST NAME");
conn.ExecCmd(cmd);


// ************* USE EXAMPLE 3 ************
using var conn = DB.GetConn("myConnString"); // opens a new Sql server connection
var query = "SELECT * FROM MYTABLE";
var dataTable = conn.ReturnData(query);

```

#### How to use - POSTGRESQL:
```csharp
using PublicUtility.Sql.Postgresql;

// ************* USE EXAMPLE 1 ************
using var conn = DB.GetConn("myConnString"); // opens a new postgresql connection
var query = new StringBuilder();
var cmd = new SqlCommand();

query.AppendLine("SELECT * FROM MYTABLE");
query.AppendLine("WHERE ID = @ID");

cmd.CommandText = query.ToString();
cmd.Parameters.AddWithValue("ID", 10);

var dataTable = conn.ReturnData(cmd);


// ************* USE EXAMPLE 2 ************
using var conn = DB.GetConn("myConnString"); // opens a new postgresql connection
var query = "UPDATE MY TABLE SET NAME = NEWNAME WHERE ID = @ID";
var cmd = new SqlCommand();

cmd.CommandText = query.ToString();
cmd.Parameters.AddWithValue("ID", 10);
cmd.Parameters.AddWithValue("NEWNAME", "IS A TEST NAME");
conn.ExecCmd(cmd);


// ************* USE EXAMPLE 3 ************
using var conn = DB.GetConn("myConnString"); // opens a new postgresql connection
var query = "SELECT * FROM MYTABLE";
var dataTable = conn.ReturnData(query);

```
#### How to use - MYSQL:
```csharp
using PublicUtility.Sql.MySql;

// CONFIGURATION FOR CONNECTING TO THE SERVER.
var builder = new MySqlConnectionStringBuilder() {
  Server = "localhost",
  Database = "dbmysql",
  UserID = "root",
  Password = "",
  Port = 3306,
  SslMode = MySqlSslMode.Required
};

// ************* USE EXAMPLE 1 ************
using var con = DB.GetConn(builder);
var dataTable = con.ReturnData("SELECT * FROM DBTESTE");

// ************* USE EXAMPLE 2 ************
using var con = DB.GetConn(builder);
var resultOk = con.ExecQuery("INSERT INTO dbTeste VALUES(default, 'test')");

// ************* USE EXAMPLE 2 ************
using var con = DB.GetConn(builder);
var query = new StringBuilder();
var cmd = new MySqlCommand();

query.Append("INSERT INTO DBTESTE VALUES ");
query.Append("  (@default, @name01)");
query.Append(", (@default, @number01)");

cmd.CommandText = query.ToString();
cmd.Parameters.AddWithValue("name01", "test01");
cmd.Parameters.AddWithValue("number01", 010101);
cmd.Parameters.AddWithValue("default", default);

var resultOk = con.ExecCmd(cmd);

```
## MongoDB Class
This class is responsible for simplifying the use of mongoDB. It does the necessary parses to convert your object into an object that mongo can interpret clearly, and it also converts objects interpreted by mongo into user-customized objects.

#### How to use - MongoDB:
```csharp
using MongoDB.Bson;
using PublicUtility.Nms.Structs;
using PublicUtility.NoSql.Mongo

// Example Get all from collection
var cli = MongoDB.GetConn("mongodb://localhost:27017"); // get mongoDB instance
cli.LoadDataAndCollection("mydatabase", "collection01"); // loads the database by pointing to a collection
var dataFromCollection = cli.GetAllFromCollection<MyOutputModel>(); // gets the collection data to be realized in your custom model

foreach(var item in dataFromCollection) 
  Console.WriteLine(item);

// Example input value on collection
cli.Insert(new MyInputModel() {
  DateTime = DateTime.Now,
  MyName = "Mr. Joseph"
});

// Example Update Field
var firstElement = cli.GetFirst<MyOutputModel>(); // gets the first element of the collection

var field = new UpdateField(nameof(firstElement.MyName), "Any Name"); // configure a field to be updated
cli.Update(firstElement._id, field); // update the configured field

// Example delete Field
cli.Delete(firstElement._id); // deletes a specific element in the collection

// Example Create and Drop collection
cli.CreateCollection("collection02"); // create a new collection
cli.DropCollection("collection02"); // drop the collection


public class MyInputModel {
  public string MyName { get; set; }
  public DateTime DateTime { get; set; }
}

public class MyOutputModel: MyInputModel {
  public ObjectId _id { get; set; }
}
```
## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License