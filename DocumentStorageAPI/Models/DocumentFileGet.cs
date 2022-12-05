using Newtonsoft.Json.Linq;

namespace DocumentStorageAPI.Models
{
    public class DocumentFileGet
    {
        public string Id { get; set; }
        public List<string> Tags { get; set; }
        public JObject Data { get; set; }
    }
}
