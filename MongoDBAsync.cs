using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using PublicUtility.Extension;
using PublicUtility.Nms.Structs;

namespace PublicUtility.NoSql.Mongo {
  public partial class MongoDB {
    public async static ValueTask<MongoDB> GetConnAsync(string connectionString, CancellationToken cancellationToken = default) => await Task.Run(MongoDB () => new(connectionString), cancellationToken);

    public async Task LoadDataAndCollectionAsync(string databaseName, string collectionName, CancellationToken cancellationToken = default) {
      await Task.Run(() => {
        _database = _mongoClient.GetDatabase(databaseName.ValueOrExeption());
        _collection = _database.GetCollection<BsonDocument>(collectionName.ValueOrExeption());
      }, cancellationToken);
    }

    public async ValueTask<IEnumerable<T>> GetAllFromCollectionAsync<T>(CancellationToken cancellationToken = default) where T : class => await GetAllAsync<T>(cancellationToken: cancellationToken);

    public async ValueTask<IEnumerable<T>> GetFromCollectionByFilter<T>(Func<T, bool> filter, CancellationToken cancellationToken = default) where T : class => await GetAllAsync(filter, cancellationToken);

    public async ValueTask<T> GetFirstAsync<T>(CancellationToken cancellationToken = default) where T : class => await Task.Run(async Task<T>() => {
      var result = await GetAllAsync<T>(cancellationToken: cancellationToken);
      return result.First();
    }, cancellationToken);

    private async ValueTask<IEnumerable<T>> GetAllAsync<T>(Func<T, bool> filter = null, CancellationToken cancellationToken = default) where T : class {
      return await Task.Run(async Task<IEnumerable<T>>() => {
        IEnumerable<T> result = null;
        try {
          var find = await _collection.FindAsync(new BsonDocument());

          result = find.ToEnumerable().Select(x => BsonSerializer.Deserialize<T>(x));

          if(filter.IsFilled())
            result = result.Where(filter);

          return result;
        } catch(Exception ex) {

          throw new Exception(ex.Message);
        }
      }, cancellationToken);
    }

    public async Task InsertAsync<T>(T value, CancellationToken cancellationToken = default) => await Task.Run(() => {
      var doc = BsonDocument.Parse(value.JsonSerialize());
      _collection.InsertOne(doc);
    }, cancellationToken);

    public async Task InsertAsync<T>(IEnumerable<T> values, CancellationToken cancellationToken = default) => await Task.Run(() => {
      IEnumerable<BsonDocument> docs = values.Select(x => BsonDocument.Parse(x.JsonSerialize()));
      _collection.InsertMany(docs);
    }, cancellationToken);

    public async Task DeleteAsync(ObjectId objectId, CancellationToken cancellationToken = default) => await Task.Run(() => {
      var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
      _collection.DeleteOne(filter);
    }, cancellationToken);

    public async Task DeleteAsync(IEnumerable<ObjectId> objectIds, CancellationToken cancellationToken = default) => await Task.Run(() => {
      foreach(var objectId in objectIds) {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
        _collection.DeleteOne(filter);
      }
    }, cancellationToken);

    public async Task UpdateAsync(ObjectId objectId, IEnumerable<UpdateField> fields, CancellationToken cancellationToken = default) => await Task.Run(() => {
      var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
      foreach(var updateField in fields) {
        var updateDefinition = Builders<BsonDocument>.Update.Set(updateField.FieldName, updateField.FieldValue);
        _collection.UpdateOne(filter, updateDefinition);
      }
    }, cancellationToken);

    public async Task UpdateAsync(ObjectId objectId, UpdateField field, CancellationToken cancellationToken = default) => await Task.Run(() => {
      var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
      var updateDefinition = Builders<BsonDocument>.Update.Set(field.FieldName, field.FieldValue);
      _collection.UpdateOne(filter, updateDefinition);
    }, cancellationToken);

    public async Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default) => await _database.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);
    public async Task DropCollectionAsync(string collectionName, CancellationToken cancellationToken = default) => await _database.DropCollectionAsync(collectionName, cancellationToken);

    public async Task RenameCollectionAsync(string oldName, string newName, CancellationToken cancellationToken = default) => await _database.RenameCollectionAsync(oldName, newName, cancellationToken: cancellationToken);
  }
}
