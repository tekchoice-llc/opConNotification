using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Amazon.Lambda.Core;

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
            getSetting.Add("channel",Environment.GetEnvironmentVariable("OpConNotificationRuleName")); //
            getSetting.Add("channel",Environment.GetEnvironmentVariable("OpConNotificationTable")); //
        }

    }
}