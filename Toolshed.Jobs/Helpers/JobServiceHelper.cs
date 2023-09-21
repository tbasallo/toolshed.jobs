using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

//namespace Toolshed.Jobs
//{
//    internal static class JobServiceHelper
//    {
//        internal static void DeleteAllEntitiesInBatches(TableClient table, string partitionKey)
//        {
//            void processor(IEnumerable<DynamicTableEntity> entities)
//            {
//                var batches = new Dictionary<string, TableBatchOperation>();

//                foreach (var entity in entities)
//                {

//                    if (batches.TryGetValue(entity.PartitionKey, out TableBatchOperation batch) == false)
//                    {
//                        batches[entity.PartitionKey] = batch = new TableBatchOperation();
//                    }

//                    batch.Add(TableOperation.Delete(entity));

//                    if (batch.Count == 100)
//                    {
//                        table.ExecuteBatch(batch);
//                        batches[entity.PartitionKey] = new TableBatchOperation();
//                    }
//                }

//                foreach (var batch in batches.Values)
//                {
//                    if (batch.Count > 0)
//                    {
//                        table.ExecuteBatch(batch);
//                    }
//                }
//            }

//            ProcessEntities(table, processor, partitionKey);
//        }
//        internal static async Task DeleteAllEntitiesInBatchesAsync(TableClient table, string partitionKey)
//        {
//            void processor(IEnumerable<DynamicTableEntity> entities)
//            {
//                var batches = new Dictionary<string, TableBatchOperation>();

//                foreach (var entity in entities)
//                {

//                    if (batches.TryGetValue(entity.PartitionKey, out TableBatchOperation batch) == false)
//                    {
//                        batches[entity.PartitionKey] = batch = new TableBatchOperation();
//                    }

//                    batch.Add(TableOperation.Delete(entity));

//                    if (batch.Count == 100)
//                    {
//                        table.ExecuteBatch(batch);
//                        batches[entity.PartitionKey] = new TableBatchOperation();
//                    }
//                }

//                foreach (var batch in batches.Values)
//                {
//                    if (batch.Count > 0)
//                    {
//                        table.ExecuteBatch(batch);
//                    }
//                }
//            }

//            await ProcessEntitiesAsync(table, processor, partitionKey);
//        }



//        internal static void ProcessEntities(TableClient table, Action<IEnumerable<DynamicTableEntity>> processor, string partitionKey)
//        {
//            TableQuerySegment<DynamicTableEntity> segment = null;

//            while (segment == null || segment.ContinuationToken != null)
//            {
//                if (string.IsNullOrWhiteSpace(partitionKey))
//                {
//                    segment = table.ExecuteQuerySegmented(new TableQuery().Take(100), segment?.ContinuationToken);
//                }
//                else
//                {
//                    // var query = new TableQuery<DynamicTableEntity>().Where(filter).Take(100);


//                    var query = new TableQuery<DynamicTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)).Take(100);
//                    //  var query = table.CreateQuery<DynamicTableEntity>().Where(filter).Take(100).AsTableQuery();
//                    segment = table.ExecuteQuerySegmented(query, segment?.ContinuationToken);
//                }

//                processor(segment.Results);
//            }
//        }


//        internal static async Task ProcessEntitiesAsync(TableClient table, Action<IEnumerable<DynamicTableEntity>> processor, string partitionKey)
//        {
//            TableQuerySegment<DynamicTableEntity> segment = null;

//            while (segment == null || segment.ContinuationToken != null)
//            {
//                if (string.IsNullOrWhiteSpace(partitionKey))
//                {
//                    segment = await table.ExecuteQuerySegmentedAsync(new TableQuery().Take(100), segment?.ContinuationToken);
//                }
//                else
//                {
//                    // var query = new TableQuery<DynamicTableEntity>().Where(filter).Take(100);


//                    var query = new TableQuery<DynamicTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)).Take(100);
//                    //  var query = table.CreateQuery<DynamicTableEntity>().Where(filter).Take(100).AsTableQuery();
//                    segment = await table.ExecuteQuerySegmentedAsync(query, segment?.ContinuationToken);
//                }

//                processor(segment.Results);
//            }
//        }

//    }
//}
