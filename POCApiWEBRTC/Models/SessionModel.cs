using System.ComponentModel.DataAnnotations.Schema;

namespace POCApiWEBRTC.Models
{
    [Table("Session")]
    public class SessionModel
    {
        [Column("Id")]
        public string? Id { get; set; }

        [Column("ApiKey")]
        public int? ApiKey { get; set; }

        [Column("ApiSecret")]
        public string? ApiSecret { get; set; }

        [Column("Token")]
        public string? Token { get; set; }
    }
}