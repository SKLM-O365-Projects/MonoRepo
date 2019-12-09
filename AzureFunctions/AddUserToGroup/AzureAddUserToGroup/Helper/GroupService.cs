using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureAddUserToGroup.Services
{
    public class GroupsService
    {

        // Get groups. 
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> GetGroups(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get groups.
            IGraphServiceGroupsCollectionPage groups = await graphClient.Groups.Request().GetAsync();

            if (groups?.Count > 0)
            {
                foreach (Group group in groups)
                {
                    items.Add(new ResultsItem
                    {
                        Display = group.DisplayName,
                        Id = group.Id
                    });
                }
            }
            return items;
        }

        public async Task<bool> PutUsersInGroup(GraphServiceClient graphClient, List<ResultsItem> users, string groupName, ILogger log)
        {
            var url = @"https://graph.microsoft.com/v1.0/groups?$filter=startswith(displayName,'" + groupName + "')";
            IGraphServiceGroupsCollectionPage groups = await new GraphServiceGroupsCollectionRequestBuilder(url, graphClient).Request().GetAsync();
            if (groups?.Count > 0)
            {
                var group = groups.FirstOrDefault();
                var getMembersUrl = @"https://graph.microsoft.com/v1.0/groups/" + group.Id + "/members";
                var members = await new DirectoryRoleMembersCollectionWithReferencesRequestBuilder(getMembersUrl, graphClient).Request().GetAsync();
                var filteredUserList = new List<ResultsItem>();
                var membersInGroup = new List<ResultsItem>();
                while (members?.Count > 0)
                {
                    foreach (var item in members)
                    {
                        membersInGroup.Add(new ResultsItem() { Id = item.Id});
                    }
                    if (members.NextPageRequest != null)
                    {
                        members = await members.NextPageRequest.GetAsync();
                    }
                    else
                    {
                        members = null;
                    }
                }

                foreach (var user in users)
                {
                    var member = membersInGroup.Where(x => x.Id == user.Id).FirstOrDefault();
                    if (member == null)
                    {
                        filteredUserList.Add(user);
                    }
                }

                foreach (var user in filteredUserList)
                {

                    try
                    {
                        var directoryObject = new DirectoryObject
                        {
                            Id = user.Id
                        };

                        await graphClient.Groups[group.Id].Members.References.Request().AddAsync(directoryObject);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex.Message);
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        
        // Get Office 365 (unified) groups. 
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> GetUnifiedGroups(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get unified groups.
            IGraphServiceGroupsCollectionPage groups = await graphClient.Groups.Request().Filter("groupTypes /any(a:a%20eq%20'unified')").GetAsync();

            if (groups?.Count > 0)
            {
                foreach (Group group in groups)
                {
                    items.Add(new ResultsItem
                    {
                        Display = group.DisplayName,
                        Id = group.Id
                    });
                }
            }
            return items;
        }

        // Get the groups that the current user is a direct member of.
        // This snippet requires an admin work account.
        public async Task<List<ResultsItem>> GetMyMemberOfGroups(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get groups the current user is a direct member of.
            IUserMemberOfCollectionWithReferencesPage memberOfGroups = await graphClient.Me.MemberOf.Request().GetAsync();

            if (memberOfGroups?.Count > 0)
            {
                foreach (var directoryObject in memberOfGroups)
                {

                    // We only want groups, so ignore DirectoryRole objects.
                    if (directoryObject is Group)
                    {
                        Group group = directoryObject as Group;
                        items.Add(new ResultsItem
                        {
                            Display = group.DisplayName,
                            Id = group.Id
                        });
                    }

                }
            }
            return items;
        }

        // Create a new group. It can be an Office 365 group, dynamic group, or security group.
        // This snippet creates an Office 365 (unified) group.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> CreateGroup(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            string guid = Guid.NewGuid().ToString();

            // Add the group.
            Group group = await graphClient.Groups.Request().AddAsync(new Group
            {
                GroupTypes = new List<string> { "Unified" },
                DisplayName = "Group" + guid.Substring(0, 8),
                Description = "Group" + guid,
                MailNickname = "group" + guid.Substring(0, 8),
                MailEnabled = false,
                SecurityEnabled = false
            });

            if (group != null)
            {

                // Get group properties.
                items.Add(new ResultsItem
                {
                    Display = group.DisplayName,
                    Id = group.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { "Description", group.Description },
                        { "Email", group.Mail },
                        { "Created", group.AdditionalData["createdDateTime"] }, // Temporary workaround for a known SDK issue.
                        { "ID", group.Id }
                    }
                });
            }
            return items;
        }

        // Get a specified group.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> GetGroup(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get the group.
            Group group = await graphClient.Groups[id].Request().Expand("members").GetAsync();

            if (group != null)
            {

                // Get group properties.
                items.Add(new ResultsItem
                {
                    Display = group.DisplayName,
                    Id = group.Id,
                    Properties = new Dictionary<string, object>
                    {
                        { "Email", group.Mail },
                        { "Member count", group.Members?.Count },
                        { "ID", group.Id }
                    }
                });
            }
            return items;
        }

        // Read properties and relationships of group members.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> GetMembers(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get group members. 
            IGroupMembersCollectionWithReferencesPage members = await graphClient.Groups[id].Members.Request().GetAsync();

            if (members?.Count > 0)
            {
                foreach (User user in members)
                {
                    // Get member properties.
                    items.Add(new ResultsItem
                    {
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

        // Read properties and relationships of group members.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> GetOwners(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Get group owners.
            IGroupOwnersCollectionWithReferencesPage members = await graphClient.Groups[id].Owners.Request().GetAsync();

            if (members?.Count > 0)
            {
                foreach (User user in members)
                {

                    // Get owner properties.
                    items.Add(new ResultsItem
                    {
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

        // Update a group.
        // This snippet changes the group name. 
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> UpdateGroup(GraphServiceClient graphClient, string id, string name)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Update the group.
            await graphClient.Groups[id].Request().UpdateAsync(new Group
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

        // Delete a group. Warning: This operation cannot be undone.
        // This snippet requires an admin work account. 
        public async Task<List<ResultsItem>> DeleteGroup(GraphServiceClient graphClient, string id)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Delete the group.
            await graphClient.Groups[id].Request().DeleteAsync();

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
    }
}