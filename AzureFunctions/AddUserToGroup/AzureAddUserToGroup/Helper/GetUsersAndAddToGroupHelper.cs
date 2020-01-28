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
            var usersOfficeManagerSE = await userService.GetAllOfficeManagers(graphClient, "Kontorschef", "SE");
            var usersOfficeManagerES = await userService.GetAllOfficeManagers(graphClient, "Kontorschef", "ES");
            var usersFranchiseesSE = await userService.GetAllOfficeManagers(graphClient, "Franchisetagare", "SE");
            var usersFranchiseesES = await userService.GetAllOfficeManagers(graphClient, "Franchisetagare", "ES");

            if (usersOfficeManagerSE.Count != 0)
            {
                await groupsService.PutUsersInGroup(graphClient, usersOfficeManagerSE, "SP_Kontorschef", log);
            }
            if (usersOfficeManagerES.Count != 0)
            {
                //Byt till utlands AD grupp 
                //await groupsService.PutUsersInGroup(graphClient, usersOfficeManagerSE, "SP_Kontorschef", log);
            }
            if (usersFranchiseesSE.Count != 0)
            {
                await groupsService.PutUsersInGroup(graphClient, usersOfficeManagerSE, "SP_Francheistagare", log);
            }
            if (usersFranchiseesES.Count != 0)
            {
                //Byt till utlands AD grupp 
                //await groupsService.PutUsersInGroup(graphClient, usersOfficeManagerSE, "SP_Kontorschef", log);
            }
            //var group = await groupsService.PutUsersInGroup(graphClient, usersOfficeManagerSE, "SP_Kontorschef", log);

            //var spainUsers = await userService.GetAllCountryUsers(graphClient, "Spanien");

            //var spainUsers = await userService.GetAllCountryUsers(graphClient, "Portugal");
            return true;
        }
        
    }
}
