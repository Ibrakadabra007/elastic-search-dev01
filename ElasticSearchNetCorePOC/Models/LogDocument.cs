using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearchNetCorePOC.Models
{
    public class LogDocument
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Body { get; set; }

        public DateTime UploadDate { get; set; }
    }
}
