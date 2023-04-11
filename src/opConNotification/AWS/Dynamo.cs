using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;

namespace opConNotification
{
    public class Dynamo
    {
        private static string region = GlobalSettings.getSetting["AWSRegion"];
        private static readonly Amazon.RegionEndpoint DynamoDBRegion = Amazon.RegionEndpoint.GetBySystemName(region);
        public static AmazonDynamoDBConfig dynamoDbConfig = new AmazonDynamoDBConfig();
        private static readonly IAmazonDynamoDB _dynamoDbClient = new AmazonDynamoDBClient(dynamoDbConfig);

        public static async Task<bool> addItem(Object obj, string table)
        {
            string objValue;
            Dictionary<string, AttributeValue> item = new Dictionary<string, AttributeValue>();
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                object objData = obj.GetType().GetProperty(property.Name).GetValue(obj, null);
                objValue = objData?.ToString() ?? "";
                item.Add(property.Name, new AttributeValue { S = objValue });
            }
            var request = new PutItemRequest
            {
                TableName = table,
                ReturnValues = "ALL_OLD",
                Item = item
            };            
            try
            {
                var DynamoDBresponse = await _dynamoDbClient.PutItemAsync(request);
                return true;
            }
            catch (ConditionalCheckFailedException e)
            {
                return false;                
            }
            catch (InternalServerErrorException e)
            {
                return false;                
            }
            catch (ItemCollectionSizeLimitExceededException e)
            {
                return false;                
            }
            catch (ProvisionedThroughputExceededException e)
            {
                return false;                
            }
            catch (RequestLimitExceededException e)
            {
                return false;                
            }
            catch (ResourceNotFoundException e)
            {
                return false;                
            }
            catch (TransactionConflictException e)
            {
                return false;                
            }
        }

        public static async Task<bool> deleteItem(string key, string table)
        {
            var request = new DeleteItemRequest
            {
                TableName = table,
                ReturnValues = "ALL_OLD",
                Key = new Dictionary<string, AttributeValue>() { { "Id", new AttributeValue { S = key } } },
            };
            try
            {
                var response = await _dynamoDbClient.DeleteItemAsync(request);
                return true;
            }
            catch (ConditionalCheckFailedException e)
            {
                return false;                
            }
            catch (InternalServerErrorException e)
            {
                return false;                
            }
            catch (ItemCollectionSizeLimitExceededException e)
            {
                return false;                
            }
            catch (ProvisionedThroughputExceededException e)
            {
                return false;                
            }
            catch (RequestLimitExceededException e)
            {
                return false;                
            }
            catch (ResourceNotFoundException e)
            {
                return false;                
            }
            catch (TransactionConflictException e)
            {
                return false;                
            }           
        }

        public static async Task<string> getItem(string key, string table)
        {            
            var request = new GetItemRequest
            {
                TableName = table,
                Key = new Dictionary<string, AttributeValue>()
                {
                    { "Id", new AttributeValue {S = key} }
                },
                ConsistentRead = true
            };
            var response = await _dynamoDbClient.GetItemAsync(request);
            return (response.IsItemSet)?JsonConvert.SerializeObject(response.Item): null;
        }

        public static async Task<string> getItemV2(string Id,string key, string table)
        {            
            var request = new GetItemRequest
            {
                TableName = table,
                Key = new Dictionary<string, AttributeValue>()
                {
                    { Id, new AttributeValue {S = key} }
                },
                ConsistentRead = true
            };
            var response = await _dynamoDbClient.GetItemAsync(request);
            return (response.IsItemSet)?JsonConvert.SerializeObject(response.Item): null;
        }


        public static async Task<bool> uptdateItemV2(string key, Dictionary<string, string> dic, string table)                
        {
            Dictionary<string, AttributeValueUpdate> attributeToUpdates = DictionaryToDynamoAttributes(dic);
            var request = new UpdateItemRequest
            {
                TableName = table,
                Key = new Dictionary<string, AttributeValue>() { { "Id", new AttributeValue { S = key } } },
                AttributeUpdates = attributeToUpdates,
                ReturnValues = "ALL_NEW",
            };
            var response = await _dynamoDbClient.UpdateItemAsync(request);          
            return (response.HttpStatusCode.ToString()=="OK")?true:false;
        }

        public static Dictionary<string, AttributeValueUpdate> DictionaryToDynamoAttributes(Dictionary<string, string> dic)
        {
            Dictionary<string, AttributeValueUpdate> dynamoAttributes = new Dictionary<string, AttributeValueUpdate>();
            foreach(KeyValuePair<string, string> entry in dic)
            {
                AttributeValueUpdate attributeValueUpdate = new AttributeValueUpdate {
                    Action = AttributeAction.PUT,
                    Value = new AttributeValue { S = entry.Value }
                };
                dynamoAttributes.Add(entry.Key,attributeValueUpdate);
            }
            return dynamoAttributes;
        }

        public static async Task<List<Dictionary<string, string>>> getAllItemsWithFilter(string table, string filterExpression, Dictionary<string,string> filterDictionary) 
        {           
            Dictionary<string, AttributeValue> lastEvaluatedKey = null;
            int lastEvaluatedKeyCount = 0;
            ScanResponse response = null;
            Dictionary<string,AttributeValue> expressionAttributeValues = new Dictionary<string, AttributeValue>();
            foreach(var filterItem  in filterDictionary)
            {
                expressionAttributeValues.Add(filterItem.Key, new AttributeValue {S = filterItem.Value});
            }
            List<Dictionary<string, string>> itemList = new List<Dictionary<string, string>>();            
            do
            {
                var request = new ScanRequest
                {
                    TableName = table,
                    ExclusiveStartKey = lastEvaluatedKey,
                    ExpressionAttributeValues = expressionAttributeValues,
                    FilterExpression = filterExpression,
                    Limit = 300,
               };

                response = await _dynamoDbClient.ScanAsync(request);
                List<Dictionary<string, AttributeValue>> items = response.Items;
               
                foreach (Dictionary<string, AttributeValue> item in items)
                {
                    Dictionary<string, string> itemDic = new Dictionary<string, string>();
                    foreach (var keyValuePair in item)
                    {
                         itemDic.Add(keyValuePair.Key,keyValuePair.Value.S);                     
                    }
                    itemList.Add(itemDic);
                }

                lastEvaluatedKey = response.LastEvaluatedKey;
                lastEvaluatedKeyCount = (lastEvaluatedKey != null)?lastEvaluatedKey.Count:0;

            } while (lastEvaluatedKey != null && lastEvaluatedKeyCount != 0);

            return itemList;
        }


        public static async Task<bool> uptdateExternalTokenItem(string key, Dictionary<string, string> token, Dictionary<string, string> tokenAdditionalData, string table)
        {

            Dictionary<string, AttributeValueUpdate> attributeToUpdates = new Dictionary<string, AttributeValueUpdate> {
                    {
                        "externalAccessToken",
                        new AttributeValueUpdate {
                            Action = AttributeAction.PUT,
                            Value = new AttributeValue { S = token["access_token"] }
                        }
                    },
                    {
                        "externalIDToken",
                        new AttributeValueUpdate {
                            Action = AttributeAction.PUT,
                            Value = new AttributeValue { S = token["id_token"] }
                        }
                    },
                    {
                        "externalRefreshToken",
                        new AttributeValueUpdate {
                            Action = AttributeAction.PUT,
                            Value = new AttributeValue { S = token["refresh_token"] }
                        }
                    }
               };

            foreach(KeyValuePair<string,string> item in tokenAdditionalData)
            {
                attributeToUpdates.Add(item.Key, new AttributeValueUpdate { Action = AttributeAction.PUT, Value = new AttributeValue { S = item.Value } });
            }

            var request = new UpdateItemRequest
            {
                TableName = table,
                Key = new Dictionary<string, AttributeValue>() { { "Id", new AttributeValue { S = key } } },
                AttributeUpdates = attributeToUpdates,
                ReturnValues = "ALL_NEW",
            };
            var response = await _dynamoDbClient.UpdateItemAsync(request);            
            return (response.HttpStatusCode.ToString()=="OK")?true:false;
        }

    }
}
