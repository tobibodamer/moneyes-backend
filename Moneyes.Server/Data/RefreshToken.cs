using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moneyes.Server.Data
{
    public class RefreshToken
    {
        public string User { get; set; }
        public string Token { get; set; }
        public DateTimeOffset Expires { get; set; }

        public RefreshToken(string user, string token, DateTimeOffset expires)
        {
            User = user;
            Token = token;
            Expires = expires;
        }
    }
}