using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AzureAddUserToGroup.Helper;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureAddUserToGroup
{
    public static class AddUserToGroup
    {
        [FunctionName("AddUserToGroup")]
        public static async void Run([TimerTrigger("0 0 0 * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("", null);
            _ = await GetUsersAndAddToGroupHelper.GetUsersAndAddToGroup(log);
           
        }
    }
}
