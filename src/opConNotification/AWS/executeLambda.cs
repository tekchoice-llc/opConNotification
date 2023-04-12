using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Lambda.Core;

namespace opConNotification
{
    [DataContract]

    public class RequestRunLambda
    {
        [DataMember]
        public string FunctionName { get; set; }
        [DataMember]
        public string JasonToLambda { get; set; }
    }

    public class ResponseRunLambda
    {
        [DataMember]
        public int StatusCode  { get; set; }
        [DataMember]
        public string Body { get; set; }
    }
    public class executeLambda
    {
        public static async Task<ResponseRunLambda> Run(RequestData requestData)
        {            
            LambdaLogger.Log("Json to Lambda" + JsonConvert.SerializeObject(requestData));
            AmazonLambdaClient lambdaClient = new AmazonLambdaClient();
            InvokeRequest lambdaRequest = new InvokeRequest
            {
                FunctionName = Environment.GetEnvironmentVariable("lambdaCall"),
                InvocationType = InvocationType.RequestResponse,
                Payload = JsonConvert.SerializeObject(requestData)
            };
            
            Task<InvokeResponse> getlambdaResponse = lambdaClient.InvokeAsync(lambdaRequest);
            InvokeResponse lambdaResponse = await getlambdaResponse;

            var streamReader = new StreamReader(lambdaResponse.Payload);
            JsonReader jsonReader = new JsonTextReader(streamReader);

            var jsonSerializer = new JsonSerializer();
            var jsonResult = jsonSerializer.Deserialize(jsonReader);

            string str = JsonConvert.SerializeObject(jsonResult);
            LambdaLogger.Log("Lambda to Json" + JsonConvert.SerializeObject(requestData));
            ResponseRunLambda responseRunLambda = JsonConvert.DeserializeObject<ResponseRunLambda>(str);
            return responseRunLambda;
        }

        public static async Task<ResponseRunLambda> RunBase(BaseRequestData requestData)
        {            
            LambdaLogger.Log("Json to Lambda" + JsonConvert.SerializeObject(requestData));
            AmazonLambdaClient lambdaClient = new AmazonLambdaClient();
            InvokeRequest lambdaRequest = new InvokeRequest
            {
                FunctionName = Environment.GetEnvironmentVariable("lambdaCall"),
                InvocationType = InvocationType.RequestResponse,
                Payload = JsonConvert.SerializeObject(requestData)
            };
            
            Task<InvokeResponse> getlambdaResponse = lambdaClient.InvokeAsync(lambdaRequest);
            InvokeResponse lambdaResponse = await getlambdaResponse;

            var streamReader = new StreamReader(lambdaResponse.Payload);
            JsonReader jsonReader = new JsonTextReader(streamReader);

            var jsonSerializer = new JsonSerializer();
            var jsonResult = jsonSerializer.Deserialize(jsonReader);

            string str = JsonConvert.SerializeObject(jsonResult);
            LambdaLogger.Log("Lambda to Json" + JsonConvert.SerializeObject(requestData));
            ResponseRunLambda responseRunLambda = JsonConvert.DeserializeObject<ResponseRunLambda>(str);
            return responseRunLambda;
        }

    }
}