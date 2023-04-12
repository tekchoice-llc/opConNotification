using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace opConNotification
{

    [DataContract]
    public class EndPointRequest
    {

        [DataMember]
        public string Id { get; set; } 
        [DataMember]
        public string requestEP { get; set; } 
        
        [DataMember]
        public string scheduleName { get; set; } 
        [DataMember]
        public string jobName { get; set; } 
        [DataMember]
        public string notificationStatus { get; set; } ="ERROR";
        [DataMember]
        public string failureDateTime { get; set; }
        [DataMember]
        public string onCallNameList { get; set; }  //ricardo, miguel, diego
        [DataMember]
        public string onCallPhoneList { get; set; } // 2109960563,2109960563,2109960563
        [DataMember]
        public string onCallName { get; set; }  //ricardo

        [DataMember]
        public string onCallPhone { get; set; }  //ricardo

        public static EndPointRequest DictionaryToObj(Dictionary<string,string> dic)
        {
            EndPointRequest endPointRequest = new EndPointRequest();
            var properties = endPointRequest.GetType().GetProperties();
            foreach(KeyValuePair<string, string> entry in dic)
            {
                foreach (var property in properties)
                {
                    if (property.Name == entry.Key)
                    {
                        endPointRequest.GetType().GetProperty(property.Name).SetValue(endPointRequest, entry.Value);
                    } 
                }             
            }
            return endPointRequest;
        }

   }

    public class LambdaEndPointRequest
    {
         [DataMember]
        public string onCallNameList { get; set; }  //ricardo, miguel, diego
        [DataMember]
        public string onCallPhoneList { get; set; } // 2109960563,2109960563,2109960563
    }
}