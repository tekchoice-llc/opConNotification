using Amazon.Connect.Model;
using Amazon.Connect;
using Amazon.Lambda.Core;

namespace opConNotification
{

    public class Connect
    {
        public static int doCall(string destinationPhoneNumber, Dictionary<string,string> paramAttributes)
        {
            CallerConfig callerConfig = setCallerConfig();
            AmazonConnectClient request = new AmazonConnectClient();            
            StartOutboundVoiceContactRequest param = new StartOutboundVoiceContactRequest();
            LambdaLogger.Log("destinationPhoneNumber:"+ destinationPhoneNumber);
            param.DestinationPhoneNumber = destinationPhoneNumber;
            //param.DestinationPhoneNumber = "+12109960563";
            param.Attributes = paramAttributes;
            param.ContactFlowId = callerConfig.ContactFlowId;
            param.InstanceId = callerConfig.InstanceId;
            param.QueueId =callerConfig.QueueId;            
            param.SourcePhoneNumber = callerConfig.SourcePhoneNumber;   
            param.TrafficType = callerConfig.TrafficType;         

            AnswerMachineDetectionConfig answerMachine = new AnswerMachineDetectionConfig();
            answerMachine.EnableAnswerMachineDetection = callerConfig.AnswerMachine; 
            answerMachine.AwaitAnswerMachinePrompt = callerConfig.AnswerMachine; 
            param.AnswerMachineDetectionConfig = answerMachine;
            Task<StartOutboundVoiceContactResponse>  requestStartOutbound = request.StartOutboundVoiceContactAsync(param); 
            try
            {
                //StartOutboundVoiceContactResponse  response = request.StartOutboundVoiceContactAsync(param).Result;   
                StartOutboundVoiceContactResponse response = requestStartOutbound.Result;
                LambdaLogger.Log("response.ContactId: " + response.ContactId +'\n');      
                LambdaLogger.Log("response.ContentLength.ToString(): " +response.ContentLength.ToString() +'\n');
                LambdaLogger.Log("response.HttpStatusCode.ToString(): " +response.HttpStatusCode.ToString()+'\n');
                return (response.HttpStatusCode.ToString()=="OK")?1:0;
            }
            catch (AggregateException e)
            {
                LambdaLogger.Log("AggregateException: " + e.Message +'\n');      
                return 403;
            }

            catch (DestinationNotAllowedException e)
            {
                LambdaLogger.Log("DestinationNotAllowedException: " + e.Message +'\n');      
                return 403;
            }
            catch (InternalServiceException e)
            {
                LambdaLogger.Log("InternalServiceException: " + e.Message +'\n');      
                return 500;
            }
            catch (InvalidParameterException e)
            {
                LambdaLogger.Log("InvalidParameterException: " + e.Message +'\n');      
                return 400;
            }
            catch (InvalidRequestException e)
            {
                LambdaLogger.Log("InvalidRequestException: " + e.Message +'\n');      
                return 400;
            }
            catch (LimitExceededException e)
            {
                LambdaLogger.Log("LimitExceededException: " + e.Message +'\n');      
                return 429;
            }
            catch (OutboundContactNotPermittedException e)
            {
                LambdaLogger.Log("OutboundContactNotPermittedException: " + e.Message +'\n');      
                return 403;
            }
            catch (ResourceNotFoundException e)
            {
                LambdaLogger.Log("ResourceNotFoundException: " + e.Message +'\n');      
                return 404;
            }
        }


        public static CallerConfig setCallerConfig()
        {
            CallerConfig callerConfig = new CallerConfig();
            callerConfig.InstanceId = Environment.GetEnvironmentVariable("InstanceId");
            callerConfig.QueueId =Environment.GetEnvironmentVariable("QueueId");
            callerConfig.OutboundCallerIdName =Environment.GetEnvironmentVariable("OutboundCallerIdName");
            callerConfig.OutboundCallerIdNumberId =Environment.GetEnvironmentVariable("OutboundCallerIdNumberId");
            callerConfig.ContactFlowId =Environment.GetEnvironmentVariable("ContactFlowId");
            callerConfig.SourcePhoneNumber =Environment.GetEnvironmentVariable("SourcePhoneNumber");
            callerConfig.AnswerMachine = (Environment.GetEnvironmentVariable("AnswerMachine")=="TRUE")?true:false;
            callerConfig.TrafficType =Environment.GetEnvironmentVariable("TrafficType"); // GENERAL or CAMPAIGN 
            return callerConfig;
        }


    }
    public class CallerConfig
    {
        public string InstanceId { get; set; }
        public string QueueId { get; set; }
        public string OutboundCallerIdName { get; set; }
        public string OutboundCallerIdNumberId { get; set; }
        public string ContactFlowId { get; set; }
        public string SourcePhoneNumber { get; set; }
        public string TrafficType { get; set; }
        public bool AnswerMachine { get; set; }
    }
}