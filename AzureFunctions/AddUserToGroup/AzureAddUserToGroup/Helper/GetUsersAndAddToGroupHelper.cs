using AzureAddUserToGroup.Helpers;
using AzureAddUserToGroup.Services;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AzureAddUserToGroup.Helper
{
    internal class GetUsersAndAddToGroupHelper
    {

        static string authority = String.Format(CultureInfo.InvariantCulture, HiddenConstants.AddInstance, HiddenConstants.Tenant);

        private static UsersService userService = new UsersService();
        private static GroupsService groupsService = new GroupsService();

        internal static async Task<bool> GetUsersAndAddToGroup(ILogger log)
        {
            GraphServiceClient graphClient = GraphHelper.GetAuthenticatedClient();
            var users = await userService.GetAllOfficeManagers(graphClient);
            var group = await groupsService.PutUsersInGroup(graphClient, users, "SP_Kontorschef", log);

            var spainUsers = await userService.GetAllCountryUsers(graphClient, "Spanien");

            //var spainUsers = await userService.GetAllCountryUsers(graphClient, "Portugal");
            return true;
        }
        
    }
}
