using System.Collections.Generic;

namespace AzureAddUserToGroup.Services
{
    public class ResultsItem
    {
        public string Display { get; internal set; }
        public string Id { get; internal set; }
        public Dictionary<string, object> Properties { get; internal set; }
    }
}