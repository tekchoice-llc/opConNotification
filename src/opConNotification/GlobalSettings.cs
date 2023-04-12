using System.Runtime.Serialization;

namespace opConNotification
{
    public static class GlobalSettings
    {
        [DataMember]
        public static Dictionary<string, string> getSetting { get; set; } 
        public static void  SetGlobalSettings()
        {
            // Get Environment Variables
            getSetting = new Dictionary<string, string>();
            getSetting.Add("AWSRegion",Environment.GetEnvironmentVariable("AWSRegion"));
            getSetting.Add("ClientNumber",Environment.GetEnvironmentVariable("ClientNumber"));
            getSetting.Add("StagingEnv",Environment.GetEnvironmentVariable("StagingEnv")); //uat-dev-prod
            getSetting.Add("channel",Environment.GetEnvironmentVariable("channel")); //
            getSetting.Add("OpConNotificationRuleName",Environment.GetEnvironmentVariable("OpConNotificationRuleName")); //
            getSetting.Add("OpConNotificationTable",Environment.GetEnvironmentVariable("OpConNotificationTable")); //
            getSetting.Add("OnCallLambda",Environment.GetEnvironmentVariable("OnCallLambda"));
        }

    }
}