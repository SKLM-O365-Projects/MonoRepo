using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Graph;

namespace AzureAddUserToGroup.Services
{
    public class UsersService
    {
        public async Task<List<ResultsItem>> GetAllCountryUsers(GraphServiceClient graphClient, string country)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            var url = @"https://graph.microsoft.com/v1.0/users?$filter=country eq '" + country + "'";
            IGraphServiceUsersCollectionPage users = await new GraphServiceUsersCollectionRequestBuilder(url, graphClient).Request().GetAsync();
            if (users?.Count > 0)
            {
                foreach (User user in users)
                {
                    items.Add(new ResultsItem
                    {
                        Display = user.DisplayName,
                        Id = user.Id
                    });
                }
            }
            return items;
        }


        public async Task<List<ResultsItem>> GetAllOfficeManagers(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            var url = @"https://graph.microsoft.com/v1.0/users?$filter=jobTitle eq 'Kontorschef'";
            IGraphServiceUsersCollectionPage users = await new GraphServiceUsersCollectionRequestBuilder(url, graphClient).Request().GetAsync();
            if (users?.Count > 0)
            {
                foreach (User user in users)
                { 
                    items.Add(new ResultsItem
                    {
                        Display = user.DisplayName,
                        Id = user.Id
                    });
                }
            }
            return items;
        }

        // Get all users.
        public async Task<List<ResultsItem>> GetAllUsers(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            IGraphServiceUsersCollectionPage users = await graphClient.Users.Request().GetAsync();
            if (users?.Count > 0)
            {
                foreach (User user in users)
                {
                    items.Add(new ResultsItem
                    {
                        Display = user.DisplayName,
                        Id = user.Id
                    });
                }
            }
            return items;
        }

        // Get the current user's profile.
        public async Task<List<ResultsItem>> GetMe(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get the current user's profile.
            User me = await graphClient.Me.Request().GetAsync();

            if (me != null)
            {

                // Get user properties.
                items.Add(new ResultsItem
                {
                    Display = me.DisplayName,
                    Id = me.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { "UPN", me.UserPrincipalName },
                        { "ID", me.Id }
                    }
                });
            }
            return items;
        }
       
        
        // Get the current user's photo. 
        public async Task<List<ResultsItem>> GetMyPhoto(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get my photo.
            using (Stream photo = await graphClient.Me.Photo.Content.Request().GetAsync())
            {
                if (photo != null)
                {

                    // Get byte[] for display.
                    using (BinaryReader reader = new BinaryReader(photo))
                    {
                        byte[] data = reader.ReadBytes((int)photo.Length);
                        items.Add(new ResultsItem
                        {
                            Properties = new Dictionary<string, object>
                            {
                                { "Stream", data }
                            }
                        });
                    }
                }
            }
            return items;
        }
        

        // Get a specified user.
        public async Task<List<ResultsItem>> GetUser(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get the user.
            User user = await graphClient.Users[id].Request().GetAsync();

            if (user != null)
            {

                // Get user properties.
                items.Add(new ResultsItem
                {
                    Display = user.DisplayName,
                    Id = user.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { "UPN", user.UserPrincipalName },
                        { "ID", user.Id }
                    }
                });
            }
            return items;
        }

        // Get a specified user's photo.
        public async Task<List<ResultsItem>> GetUserPhoto(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get the user's photo.
            using (Stream photo = await graphClient.Users[id].Photo.Content.Request().GetAsync())
            {
                if (photo != null)
                {

                    // Get byte[] for display.
                    using (BinaryReader reader = new BinaryReader(photo))
                    {
                        byte[] data = reader.ReadBytes((int)photo.Length);
                        items.Add(new ResultsItem
                        {
                            Properties = new Dictionary<string, object>
                            {
                                { "Stream", data }
                            }
                        });
                    }
                }
            }
            return items;
        }

        // Get the direct reports of a specified user.
        // This snippet requires an admin work account.
        public async Task<List<ResultsItem>> GetDirectReports(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get user's direct reports.
            IUserDirectReportsCollectionWithReferencesPage directs = await graphClient.Users[id].DirectReports.Request().GetAsync();

            if (directs?.Count > 0)
            {
                foreach (User user in directs)
                {

                    // Get user properties.
                    items.Add(new ResultsItem
                    {
                        Display = user.DisplayName,
                        Id = user.Id,
                        Properties = new Dictionary<string, object>
                        {
                            { "UPN", user.UserPrincipalName },
                            { "ID", user.Id }
                        }
                    });
                }
            }
            return items;
        }

        // Update a user.
        // This snippet changes the user's display name. 
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> UpdateUser(GraphServiceClient graphClient, string id, string name)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Update the user.
            await graphClient.Users[id].Request().UpdateAsync(new User
            {
                DisplayName = "Updated " + name
            });

            items.Add(new ResultsItem
            {

                // This operation doesn't return anything.
                Properties = new Dictionary<string, object>
                {
                    { "Operation completed. This call doesn't return anything.", "" }
                }
            });
            return items;
        }

        // Delete a user. Warning: This operation cannot be undone.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> DeleteUser(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            ResultsItem item = new ResultsItem();

            // Make sure that the current user is not selected.
            User me = await graphClient.Me.Request().Select("id").GetAsync();
            if (id == me.Id)
            {
                item.Properties.Add("Please choose another user. This snippet doesn't support deleting the current user.", "");
            }
            else
            {

                // Delete the user.
                await graphClient.Users[id].Request().DeleteAsync();

                // This operation doesn't return anything.
                item.Properties.Add("Operation completed. This call doesn't return anything.", "");
            }
            items.Add(item);
            return items;
        }
    }

}