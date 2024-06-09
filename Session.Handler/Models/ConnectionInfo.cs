using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Session.Handler.Models
{
    public class ConnectionInfo
    {
        public string Url { get; set; }

        public string AccessToken { get; set; }

        public ConnectionInfo(string url, string accessToken)
        {
            Url = url;
            AccessToken = accessToken;
        }
    }
}
