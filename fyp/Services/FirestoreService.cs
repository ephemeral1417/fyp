using fyp.Models;
using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fyp.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        // Generic method to add a document to a collection
        public async Task<string> AddDocumentAsync<T>(string collectionName, T data)
        {
            var docRef = await _firestoreDb.Collection(collectionName).AddAsync(data);
            return docRef.Id;
        }

        // Generic method to set a document with a specific ID
        public async Task SetDocumentAsync<T>(string collectionName, string documentId, T data)
        {
            await _firestoreDb.Collection(collectionName).Document(documentId).SetAsync(data);
        }

        // Generic method to get a document by ID
        public async Task<T> GetDocumentAsync<T>(string collectionName, string documentId)
        {
            var docSnapshot = await _firestoreDb.Collection(collectionName).Document(documentId).GetSnapshotAsync();

            if (docSnapshot.Exists)
            {
                return docSnapshot.ConvertTo<T>();
            }

            return default;
        }

        // Generic method to get all documents from a collection
        public async Task<List<T>> GetAllDocumentsAsync<T>(string collectionName)
        {
            var snapshot = await _firestoreDb.Collection(collectionName).GetSnapshotAsync();
            return snapshot.Documents.Select(d => d.ConvertTo<T>()).ToList();
        }

        // Generic method to update a document
        public async Task UpdateDocumentAsync<T>(string collectionName, string documentId, T data)
        {
            await _firestoreDb.Collection(collectionName).Document(documentId).SetAsync(data, SetOptions.MergeAll);
        }

        // Generic method to delete a document
        public async Task DeleteDocumentAsync(string collectionName, string documentId)
        {
            await _firestoreDb.Collection(collectionName).Document(documentId).DeleteAsync();
        }

        // Get documents with a filter
        public async Task<List<T>> GetDocumentsWithFilterAsync<T>(string collectionName, string field, object value)
        {
            var query = _firestoreDb.Collection(collectionName).WhereEqualTo(field, value);
            var querySnapshot = await query.GetSnapshotAsync();
            return querySnapshot.Documents.Select(d => d.ConvertTo<T>()).ToList();
        }
    }
}