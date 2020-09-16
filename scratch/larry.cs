using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Linq;

namespace RatingsAPI
{
    public static class CreateRating
    {
        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("CreateRating function processed a request.");

            string rating = req.Query["rating"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if(data != null)
            {
                int lclrating;
                string userId = data?.userId;
                string productId = data?.productId;
                //grab rating as a string so it can be tested to see if its an int
                string lclratingstr = data?.rating;

                //If rating is an int, check the range
                if((lclratingstr.All(char.IsDigit)))
                {
                    lclrating = int.Parse(lclratingstr);
                    // Enumerable.Range requires max + 1
                    if (!(Enumerable.Range(0, 6).Contains(lclrating)))
                        return new BadRequestObjectResult("Rating needs to be between 0 and 5");
                }
                else
                {
                    return new BadRequestObjectResult("Rating needs to be an integer");

                }


                HttpClient newClient = new HttpClient();
                HttpRequestMessage userIdRequest = new HttpRequestMessage(HttpMethod.Get, string.Format("https://serverlessohuser.trafficmanager.net/api/GetUser?userId={0}", userId));
                HttpResponseMessage uidresponse = await newClient.SendAsync(userIdRequest);
                string retUserInfo = await uidresponse.Content.ReadAsStringAsync();
                if (uidresponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return new BadRequestObjectResult(retUserInfo);
                }

                HttpRequestMessage prodIdRequest = new HttpRequestMessage(HttpMethod.Get, string.Format("https://serverlessohproduct.trafficmanager.net/api/GetProduct?productId={0}", productId));
                HttpResponseMessage pidresponse = await newClient.SendAsync(prodIdRequest);
                string retProdInfo = await pidresponse.Content.ReadAsStringAsync();
                if (pidresponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return new BadRequestObjectResult(retProdInfo);
                }

                data["Values"]["id"] = new System.Guid();
                
                data["Values"]["timestamp"] = DateTime.UtcNow;


            }

            rating = rating ?? data?.rating;

            string responseMessage = string.IsNullOrEmpty(rating)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {rating}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
   

