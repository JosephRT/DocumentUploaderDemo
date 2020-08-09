using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DocumentUploadCore.Data;
using DocumentUploadCore.Entities;

namespace SQLiteDataSource
{
    public class SQLiteDocumentRepository : IDocumentRepository
    {
        private readonly IDbConnection databaseConnection;


        public SQLiteDocumentRepository(IDbConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }


        public async Task DeleteDocumentAsync(int documentToDelete)
        {
            const string deleteDocumentSql = @"
                DELETE FROM [managedDocuments]
                WHERE managedDocumentId = @id;";

            await databaseConnection.ExecuteAsync(deleteDocumentSql, new {id = documentToDelete});
        }

        public async Task<ManagedDocument> GetDocumentAsync(int documentToRetrieve)
        {
            const string retrieveDocumentSql = @"
                SELECT managedDocumentId as id, created, name, fileType, contents 
                FROM [managedDocuments] 
                WHERE Id = @id;";

            var retrievedDocument = await databaseConnection.QueryAsync<ManagedDocumentMetadata, ManagedDocument, ManagedDocument>(retrieveDocumentSql,((m, d) =>
                {
                    d.Metadata = m;
                    return d;
                }),
                splitOn: "contents",
                param: new { id = documentToRetrieve});
            return retrievedDocument.FirstOrDefault();
        }

        public async Task<IList<ManagedDocumentMetadata>> ListDocumentsAsync()
        {
            const string listDocumentsMetadataSql = @"
                SELECT managedDocumentId as id, created, name, fileType
                FROM [managedDocuments]";

            var documentsMetadata =  await databaseConnection.QueryAsync<ManagedDocumentMetadata>(listDocumentsMetadataSql);
            return documentsMetadata.ToList();
        }

        public async Task<int> SaveDocumentAsync(ManagedDocument documentToSave)
        {
            const string insertDocumentSql = @"
                INSERT INTO [managedDocuments] (created, name, fileType, contents)
                VALUES (@created, @name, @fileType, @contents);
                SELECT last_insert_rowid();";

            return await databaseConnection.ExecuteScalarAsync<int>(insertDocumentSql,
                new
                {
                    created = documentToSave.Metadata.Created.ToString("yyyy-MM-dd HH:mm:ss"), 
                    name = documentToSave.Metadata.Name,
                    fileType = documentToSave.Metadata.FileType,
                    contents = documentToSave.Contents
                });
        }
    }
}
