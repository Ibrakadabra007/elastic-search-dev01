using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearchNetCorePOC.Models
{
    public class DbConnection
    {
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ServerName { get; set; }

    }
}
