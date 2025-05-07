using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace GamblingServer.DB
{
    [PrimaryKey("username")]
    public class User
    {
        
        public string username { get; set; }
        [Required]
        public byte[] passhash { get; set; }
        [Required]
        public DateTime creation_time { get; set; }
    }
}
