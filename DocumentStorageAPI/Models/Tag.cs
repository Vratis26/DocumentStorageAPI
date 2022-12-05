using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DocumentStorageAPI.Models
{
    [PrimaryKey(nameof(Id), nameof(Name))]
    public class Tag
    {
        [Key]
        public string Id { get; set; }
        [Key]
        public string Name { get; set; }
    }
}
