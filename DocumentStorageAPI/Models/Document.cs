using System.ComponentModel.DataAnnotations;

namespace DocumentStorageAPI.Models
{
    public class Document
    {
        [Key]
        public string Id { get; set; }
        public string Data { get; set; }
    }
}
