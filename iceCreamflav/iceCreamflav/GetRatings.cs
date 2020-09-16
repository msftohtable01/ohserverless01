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
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;
using System.Linq;

namespace iceCreamflav
{
    public static class GetRatings
    {
        [FunctionName("GetRatings")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
         [CosmosDB(
                databaseName: "icecreammaster",
                collectionName: "icecreamfeedback",
                ConnectionStringSetting =  "cosmosdbconn")] DocumentClient client, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string userId = req.Query["userId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            userId = userId ?? data?.userId;

            Uri userCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseId: "icecreammaster", collectionId: "icecreamfeedback");

            var options = new FeedOptions { EnableCrossPartitionQuery = true }; // Enable cross partition query

            IDocumentQuery<ratingmodel>query = client.CreateDocumentQuery<ratingmodel>(userCollectionUri, options)
                                                 .Where(rating => rating.userId == userId)
                                                 .AsDocumentQuery();

            var userRatingList = new List<ratingmodel>();

            while (query.HasMoreResults)
            {
                foreach (ratingmodel rating in await query.ExecuteNextAsync())
                {
                    userRatingList.Add(rating);
                }
            }
            if (userRatingList.Count == 0) {
                return new NotFoundObjectResult("UserId not provided or invalid");
            }
            return new OkObjectResult(userRatingList);
        }
    
    }
}
