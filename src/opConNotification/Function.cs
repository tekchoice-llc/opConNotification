using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace opConNotification
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async static Task<APIGatewayProxyResponse> handlerCore(System.IO.Stream apigProxyEvent, ILambdaContext context)
        {
            EndPointResponse endPointResponse = new EndPointResponse();
            Task<EndPointResponse> getEndpointResponse;
            string strBodyRequest;
            using (var reader = new StreamReader(apigProxyEvent, Encoding.UTF8))
            {
                strBodyRequest = reader.ReadToEnd();
            }

            EndPointRequest endPointRequest = JsonConvert.DeserializeObject<EndPointRequest>(strBodyRequest);
            GlobalSettings.SetGlobalSettings();

            if (!string.IsNullOrEmpty(endPointRequest.requestEP))
            {
                switch (endPointRequest.requestEP)
                {
                    case "newFailure":
                        Task<string> getEnableEventRule = EventBridge.enableRule(GlobalSettings.getSetting["OpConNotificationRuleName"]);
                        endPointResponse.strBody = await getEnableEventRule;

                        // OpsGenie Get data onCallNameList & onCallNamePhoneList

                        string[] onCallName = endPointRequest.onCallNameList.Split("|"); 
                        string[] onCallPhone = endPointRequest.onCallPhoneList.Split("|"); 
                        endPointRequest.onCallName = onCallName[0];
                        endPointRequest.onCallPhone = onCallPhone[0];
                        Random rand = new Random();
                        endPointRequest.Id = "Te$k" + rand.NextDouble().ToString() + "!@";

                        Task<bool> addDataToTable = Dynamo.addItem(endPointRequest, GlobalSettings.getSetting["OpConNotificationTable"]);
                        await addDataToTable;

                        // Call Staff
                        Dictionary<string,string> paramAttributes = new Dictionary<string, string>();
                        paramAttributes.Add("Id",endPointRequest.Id);
                        paramAttributes.Add("scheduleName",endPointRequest.scheduleName);
                        paramAttributes.Add("jobName",endPointRequest.jobName);
                        paramAttributes.Add("notificationStatus",endPointRequest.notificationStatus);
                        paramAttributes.Add("failureDateTime",endPointRequest.failureDateTime);
                        paramAttributes.Add("onCallName",endPointRequest.onCallName);
                        int responseInt = Connect.doCall(endPointRequest.onCallPhone,paramAttributes);

                    break;

                    case "doNotification":
                        string filterExpression = "notificationStatus = :valnotificationStatus";
                        Dictionary<string,string> filterDictionary = new Dictionary<string, string>(); 
                        filterDictionary.Add(":valnotificationStatus","ERROR");
                        Task<List<Dictionary<string, string>>> getAllItems = Dynamo.getAllItemsWithFilter(GlobalSettings.getSetting["OpConNotificationTable"],filterExpression,filterDictionary);
                        List<Dictionary<string, string>>  allItems = await getAllItems;
                        if (allItems.Count() > 0)
                        {
                            endPointRequest = EndPointRequest.DictionaryToObj(allItems[0]);
                        }

                        string[] onCallNameNextAttempt = endPointRequest.onCallNameList.Split("|"); 
                        string[] onCallPhoneNextAttempt = endPointRequest.onCallPhoneList.Split("|");
                        int MaxAttempt = onCallNameNextAttempt.Count();
                        for (int currentName=0;currentName<MaxAttempt; currentName++)
                        {
                            if (onCallNameNextAttempt[currentName] == endPointRequest.onCallName)
                            {
                                endPointRequest.onCallName = (currentName==MaxAttempt)?onCallNameNextAttempt[0]:onCallNameNextAttempt[currentName];
                                endPointRequest.onCallPhone = (currentName==MaxAttempt)?onCallPhoneNextAttempt[0]:onCallPhoneNextAttempt[currentName];
                            }
                        }
                        
                        Dictionary<string,string> saveDic = new Dictionary<string, string>();
                        saveDic.Add("onCallName",endPointRequest.onCallName);
                        saveDic.Add("onCallPhone",endPointRequest.onCallPhone);
                        Task<Boolean> saveLogRequest = Dynamo.uptdateItemV2(endPointRequest.Id,saveDic,GlobalSettings.getSetting["OpConNotificationTable"]);
                        Boolean saveLogResponse = await saveLogRequest;

                        // Call Staff
                        Dictionary<string,string> paramAttributesNextAttempt = new Dictionary<string, string>();
                        paramAttributesNextAttempt.Add("scheduleName",endPointRequest.scheduleName);
                        paramAttributesNextAttempt.Add("jobName",endPointRequest.jobName);
                        paramAttributesNextAttempt.Add("notificationStatus",endPointRequest.notificationStatus);
                        paramAttributesNextAttempt.Add("failureDateTime",endPointRequest.failureDateTime);
                        paramAttributesNextAttempt.Add("onCallName",endPointRequest.onCallName);
                        int responseIntNextAttempt = Connect.doCall(endPointRequest.onCallPhone,paramAttributesNextAttempt);
                    break;

                    case "disableEventBridge":
                        Task<string> getDisableEventRule = EventBridge.disableRule(GlobalSettings.getSetting["ruleName"]);
                        endPointResponse.strBody = await getDisableEventRule;

                    break;

                    default:
                    break;
                }
            }
            return sendResponseToClient(endPointResponse);
        }

        public static APIGatewayProxyResponse sendResponseToClient(EndPointResponse endPointResponse)
        {
            return new APIGatewayProxyResponse
            {
                Body = endPointResponse.strBody,
                StatusCode = endPointResponse.intError, 
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}

