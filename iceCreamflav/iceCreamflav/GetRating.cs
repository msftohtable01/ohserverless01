using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Linq;
using System.Net;
using System.Net.Http;

namespace iceCreamflav
{
    public static class GetRating
    {
        [FunctionName("GetRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
         [CosmosDB(
                databaseName: "icecreammaster",
                collectionName: "icecreamfeedback",
                ConnectionStringSetting =  "cosmosdbconn")] DocumentClient client, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string RatingId = req.Query["RatingId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            RatingId = RatingId ?? data?.RatingId;

            if (string.IsNullOrEmpty(RatingId))
            {
                return new NotFoundObjectResult("UserId not provided");
            }
            Uri userCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseId: "icecreammaster", collectionId: "icecreamfeedback");

            var options = new FeedOptions { EnableCrossPartitionQuery = true }; // Enable cross partition query

            IDocumentQuery<ratingmodel> query = client.CreateDocumentQuery<ratingmodel>(userCollectionUri, options)
                                                 .Where(rating => rating.id == RatingId)
                                                 .AsDocumentQuery();

            var userRatingList = new List<ratingmodel>();

            while (query.HasMoreResults)
            {
                foreach (ratingmodel rating in await query.ExecuteNextAsync())
                {
                    userRatingList.Add(rating);
                }
            }
            if (userRatingList.Count == 0)
            {
                return new NotFoundObjectResult(HttpStatusCode.NotFound);
               // return new HttpResponseMessage(HttpStatusCode.NotFound.ToString());
            }

            return new OkObjectResult(userRatingList);
        }
    
    }
}
