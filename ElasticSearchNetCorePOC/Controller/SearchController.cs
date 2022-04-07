using ElasticSearchNetCorePOC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ElasticSearchNetCorePOC.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        public SearchController(IConfiguration config)
        {
            Configuration = config;
        }

        public IConfiguration Configuration { get; }
        private string SearchHostUri { get; set; }

        [HttpGet("Host")]
        public string GetHost()
        {
            SearchHostUri = Configuration.GetSection("AppSettings").GetSection("Search-Uri").Value;
            return SearchHostUri;
        }

        // GET: api/search
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "search-uri", "http://localhost:9200" };
        }

        // GET api/search/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return $"value {id}";
        }


        /// <summary>
        ///   ex)  {name} ==  my-index-004
        /// </summary>
        /// <param name="name"></param>
        [HttpPut("CreateIndex/{name}")]
        public async Task<ICreateIndexResponse> CreateIndex(string name)
        {
            string indexName = name ?? GetIndexNameFromPath(@"Data\WorldLocations.json");
            //string indexName = name ?? GetIndexNameFromPath(@"Data\divina_commedia.txt");

            SearchHostUri = Configuration.GetSection("AppSettings").GetSection("Search-Uri").Value;
            ElasticClient client = GetConnection(SearchHostUri, indexName);
            ////CreateIndex(client, "divina_commedia.txt", indexName);
            var response = await CreateIndex(client,  indexName);

            return response;

        }

        [HttpGet("Query/{query}")]
        public IActionResult Query(string query)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            string indexName = GetIndexNameFromPath(@"Data\WorldLocations.json");

            SearchHostUri = Configuration.GetSection("AppSettings").GetSection("Search-Uri").Value;
            ElasticClient client = GetConnection(SearchHostUri, indexName);

            var queryResult = SearchString(client, query);



            if (queryResult.Count == 0 || queryResult == null)
            {
                return NotFound();
            }

            return Ok(queryResult);

        }

        //[HttpPut("Query/{name}, {query}")]
        //public void Query(string name, string query)
        //{
        //string indexName = name ?? GetIndexNameFromPath("divina_commedia.txt");

        //ElasticClient client = GetConnection(_searchHostUri, indexName);

        //SearchString(client, query);

        //}


        // Note: - from autoship api-service
        //
        // GET: api/Subscriptions/1797B430-1B4A-EB11-98A2-74F06DB18F99
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetSubscription([FromRoute] Guid id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var subscription = await _context.Subscription.FindAsync(id);

        //    if (subscription == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(subscription);
        //}




        //// PUT api/<SearchController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}



        //// DELETE api/<SearchController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}

        #region helpers

        /// <summary>
        ///   -When creating an index, you can specify the following:
        ///     1.Settings for the index  2.Mappings for fields in the index 3.Index aliases
        ///     ex)  my-index-004
        /// </summary>
        /// <param name="client"></param>
        /// <param name="filepath"></param>
        /// <param name="indexName"></param>
        private void CreateIndex(ElasticClient client, string filepath, string indexName)
        {

            Stopwatch timer = new Stopwatch();
            timer.Start();

            var doesIndexExist = client.IndexExists(indexName, i => i.Index(indexName));

            if (!doesIndexExist.Exists)
            {
                client.CreateIndexAsync(indexName, c => c.Index(indexName));

            }

            //CreateIndexResponse createIndexResponse = client.Indices.Create(indexName, c => c.Map<LogDocument>(m => m.AutoMap<LogDocument>()));

            string[] lines = System.IO.File.ReadAllLines(filepath);

            int items = 0;
            Parallel.ForEach(lines, (line) =>
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    client.CreateDocument<LogDocument>(new LogDocument()
                    {
                        Body = line.Trim(),
                        UploadDate = DateTime.Now
                    });
                    items++;
                }
            });

            timer.Stop();
            Debug.WriteLine($"Index created in {timer.ElapsedMilliseconds}ms with {items} element.");
        }
        private async Task<ICreateIndexResponse> CreateIndex(ElasticClient client, string indexName)
        {

            ICreateIndexResponse resp = null;

            var doesIndexExist = client.IndexExists(indexName, i => i.Index(indexName));

            if (!doesIndexExist.Exists)
            {
                resp = await client.CreateIndexAsync(indexName, c => c.Index(indexName));

            }

            //CreateIndexResponse createIndexResponse = client.Indices.Create(indexName, c => c.Map<LogDocument>(m => m.AutoMap<LogDocument>()));

            return resp;

        }



        //private void SearchString(ElasticClient client, string searchStr)
        //{

        //    Debug.WriteLine($"Searching for ${searchStr}");
        //    ISearchResponse<LogDocument> searchResponse =
        //        client.Search<LogDocument>(s => s.Size(10).Query(q => q.QueryString(
        //                                    qs => qs.Query(searchStr)
        //                                    .AllowLeadingWildcard(true))));

        //    IReadOnlyCollection<LogDocument> docs = searchResponse.Documents;

        //    foreach (LogDocument doc in docs)
        //    {
        //        Debug.WriteLine(doc.Body);
        //    }
        //}

        private List<LogDocument> SearchString(ElasticClient client, string searchStr)
        {

            Console.WriteLine($"Searching for ${searchStr}");
            ISearchResponse<LogDocument> searchResponse =
                client.Search<LogDocument>(s => s.Size(10).Query(q => q.QueryString(
                                           qs => qs.Query(searchStr).AllowLeadingWildcard(true))));
                                           //{
                                           //    qs.Query(searchStr).AllowLeadingWildcard(true);
                                           //    qs.Name("");
                                           //    qs.Boost(1.3);
                                           //    return qs;
                                           //})));

            var docs = searchResponse.Documents;

            return docs.ToList();
        }

        private ElasticClient GetConnection(string connectionUri, string indexname)
        {
            Uri node = new Uri(connectionUri);
            ConnectionSettings settings = new ConnectionSettings(node).DefaultMappingFor<LogDocument>(m => m.IndexName(indexname));
            //settings.PrettyJson(true);
            //settings.RequestTimeout(new TimeSpan(30000));

            return new ElasticClient(settings);
        }

        private string GetIndexNameFromPath(string filepath)
        {
            string basepath = Path.GetFileNameWithoutExtension(filepath).ToLower();
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            return rgx.Replace(filepath, "");
        }

        #endregion

    }
}
