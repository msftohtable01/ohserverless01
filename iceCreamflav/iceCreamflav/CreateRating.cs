using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using System.Net.Http;

namespace iceCreamflav
{
    public static class CreateRating
    {
        //public const string cosmosdbconn = "AccountEndpoint=https://icecreamcosmos.documents.azure.com:443/;AccountKey=R0zNEmDpPZBTDyGUpN1OK4hdR77FFPiT0KwaPSgoJX4GBVSPA9V2wwt62kbjWHAmmQD8UwYzt6i2eMN8cJJP9w==;";

        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "icecreammaster",
                collectionName: "icecreamfeedback",
                ConnectionStringSetting =  "cosmosdbconn")] IAsyncCollector<object> taskItems, ILogger log)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ratingmodel input = JsonConvert.DeserializeObject<ratingmodel>(requestBody);


            //Validate Product
            System.Net.Http.HttpClient newClient = new HttpClient();
            HttpRequestMessage newRequest = new HttpRequestMessage(HttpMethod.Get, string.Format("https://serverlessohproduct.trafficmanager.net/api/GetProduct?ProductId={0}", input.productId));

            HttpResponseMessage response = await newClient.SendAsync(newRequest);
           string productObj = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(productObj);
          
            if (data?.productId == null)
            {
                return new NotFoundObjectResult("product dont exist");
            }

            ////Validate user
            System.Net.Http.HttpClient userClient = new HttpClient();
            HttpRequestMessage userRequest = new HttpRequestMessage(HttpMethod.Get, string.Format("https://serverlessohuser.trafficmanager.net/api/GetUser?userId={0}", input.userId));

            HttpResponseMessage userresponse = await userClient.SendAsync(userRequest);
            string userObj = await userresponse.Content.ReadAsStringAsync();
            dynamic userdata = JsonConvert.DeserializeObject(userObj);
            if (userdata?.userId == null)
            {
                return new NotFoundObjectResult("user dont exist");
            }


           // Guid Id = Guid.NewGuid();
          
           object taskItem = new
            {
               input.productId,
               input.locationName,
               input.userId,
               input.rating,
               input.userNotes,
           };

            await taskItems.AddAsync(taskItem);

            return new OkObjectResult(taskItem);
        }
    }
  
}
