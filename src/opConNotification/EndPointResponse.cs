using System.Runtime.Serialization;


namespace opConNotification
{

    [DataContract]
    public class EndPointResponse
    {
        [DataMember]
        public string strBody { get; set; } = "Invalid Request";
        [DataMember]
        public int intError { get; set; } = 400;
    }
}