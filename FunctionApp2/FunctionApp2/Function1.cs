using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.Text;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace FunctionApp2
{
    public static class processorder
    {
        [FunctionName("processorder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            // dynamic dataArray1 = JsonConvert.Deserialize(requestBody);
            //Collection<payload> dataArray = 
            Collection<payload> dataArray = JsonConvert.DeserializeObject<Collection<payload>>(requestBody);
            // Collection<payload> dataArray = await new StreamReader(req.Body).ReadToEndAsync();
            //Collection<payload> dataArray = JObject.Parse(await new StreamReader(req.Body).ReadToEndAsync());


            for (var i = 0; i < dataArray.Count; i++)
            {
                payload salesheader = new payload();
                salesheader.totalItems = dataArray[i].totalItems;
                salesheader.totalCost = dataArray[i].totalCost;
                salesheader.salesNumber = dataArray[i].salesNumber;
                salesheader.salesDate = dataArray[i].salesDate;
                salesheader.storeLocation = dataArray[i].storeLocation;
                salesheader.receiptUrl = dataArray[i].receiptUrl;

                if (salesheader.totalCost > 100)
                {
                    byte[] data;
                    using (var webClient = new WebClient())
                        data = webClient.DownloadData(salesheader.receiptUrl);
                    string enc = Convert.ToBase64String(data);
                    string urlenc = HttpUtility.UrlEncode(enc);
                    salesheader.ReceiptImage = urlenc;
                    await CreateBlob(salesheader.salesNumber, JsonConvert.SerializeObject(salesheader), "high");

                    //result = HttpStatusCode.OK;
                }
                else
                {
                    await CreateBlob(salesheader.salesNumber, JsonConvert.SerializeObject(salesheader), "low");
                }

            }

            // string requestBody = await new StreamReader(req.Body).ReadToEndAsync();



            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
        private async static Task CreateBlob(string name, string data, string containerflag)
        {
            string accessKey;
            string accountName;
            string connectionString;
            CloudStorageAccount storageAccount;
            CloudBlobClient client;
            CloudBlobContainer container;
            CloudBlockBlob blob;

            accessKey = ConfigurationManager.AppSettings["StorageAccessKey"];
            accountName = ConfigurationManager.AppSettings["StorageAccountName"];
            //connectionString = "DefaultEndpointsProtocol=https;AccountName=" + accountName + ";AccountKey=" + accessKey + ";EndpointSuffix=core.windows.net";
            connectionString = "DefaultEndpointsProtocol=https;AccountName=icecreamhack6;AccountKey=VQ7s6NBksZehiyOINtge7+fGOs7LtmtZ9t86EFdXjGRPIocksjezZ4mZQj8HCEB77RnV+fxtoWXz2UELLFA1Sw==;EndpointSuffix=core.windows.net";
            storageAccount = CloudStorageAccount.Parse(connectionString);

            client = storageAccount.CreateCloudBlobClient();
            if (containerflag == "high")
            {
                container = client.GetContainerReference("receipts-high-value");
            }
            else
            {
                container = client.GetContainerReference("receipts");
            }


            await container.CreateIfNotExistsAsync();

            blob = container.GetBlockBlobReference(name);
            blob.Properties.ContentType = "application/json";

            //using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            // using (Stream stream = new MemoryStreamdata)))
            // {
            await blob.UploadTextAsync(data);
            // }
        }
    }
    public class payload
    {
        public int totalItems { get; set; }
        public double totalCost { get; set; }
        public string salesNumber { get; set; }
        public string salesDate { get; set; }
        public string storeLocation { get; set; }
        public string receiptUrl { get; set; }
        public string ReceiptImage { get; set; }
    }


}
