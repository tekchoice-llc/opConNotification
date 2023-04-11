using Amazon.EventBridge;
using Amazon.EventBridge.Model;

namespace opConNotification
{

    public class EventBridge
    {
        private static readonly AmazonEventBridgeClient clientEvent = new AmazonEventBridgeClient();

        public static async Task<string> enableRule(string ruleName)
        {
            if (!String.IsNullOrEmpty(ruleName))
            {
                var request = new EnableRuleRequest
                {
                    Name = ruleName
                };

                var response = await clientEvent.EnableRuleAsync(request);
                Console.WriteLine("enable response ----> {0}", response.HttpStatusCode);

                return response.HttpStatusCode.ToString();
            }

            return "Fail";
        }
        public static async Task<string> disableRule(string ruleName)
        {
            if (!String.IsNullOrEmpty(ruleName))
            {
                var request = new DisableRuleRequest
                {
                    Name = ruleName
                };
                var response = await clientEvent.DisableRuleAsync(request);
                Console.WriteLine("disable response ----> {0}", response.HttpStatusCode);

                return response.HttpStatusCode.ToString();
            }

            return "Fail";
        }

    }
}