using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;

namespace DocumentStorageAPI.Models
{
    public class DocumentFile
    {
        public string Id { get; set; }
        public List<string> Tags { get; set; }
        public JsonObject Data { get; set; }
    }
}
