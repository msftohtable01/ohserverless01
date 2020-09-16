using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ohserverless04_func
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string productId = req.Query["productId"];
            log.LogInformation($"Query String is: {productId}");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            productId = productId ?? data?.productId;

            log.LogInformation($"ProductId  is: {productId}");
            string responseMessage = string.IsNullOrEmpty(productId)
                ? "No productId passed in"
                : $"The product name for your product id {productId} is Starfruit Explosion";

            return new OkObjectResult(responseMessage);
        }
    }
}
