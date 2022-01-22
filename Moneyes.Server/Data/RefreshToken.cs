using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moneyes.Server.Data
{
    public class RefreshToken
    {
        [Required]
        public string User { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTimeOffset Expires { get; set; }

        public string? AppId { get; set; }

        public RefreshToken(string user, string token, DateTimeOffset expires, string? appId)
        {
            User = user;
            Token = token;
            Expires = expires;
            AppId = appId;
        }
    }
}