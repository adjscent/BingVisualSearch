using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;

/* This sample makes a call to the Bing Visual Search API with a query image and returns similar images with details.
 * Bing Visual Search API: 
 * https://docs.microsoft.com/en-us/rest/api/cognitiveservices/bingvisualsearch/images/visualsearch
 */

namespace BingVisualSearch
{
    internal class Program
    {
        // Set the path to the image
        private static string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string imagePath = 
            @"file://" + executableLocation + "/objects.jpg";
        // Set your access key
        private static string accessKey = "";
        private static Uri endpointUrl = new Uri("https://api.cognitive.microsoft.com/bing/v7.0/images/visualsearch");

        private static HttpClient client;
        private static Uri imgUri;

        private static void Main()
        {
            //define change this to receive a URL if you want
            imgUri = new Uri(imagePath);

            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", accessKey);

            var request =
                new
                {
                    imageInfo = new
                    {
                        cropArea = new
                        {
                            top = 0.0,
                            left = 0.0,
                            right = 0.0,
                            bottom = 0.0
                        },
                        url = imgUri.IsFile ? (string) null : imgUri.ToString()
                    }
                };
            var mfdc = new MultipartFormDataContent();

            // Part #2 - Add binary image file if using a local image
            // NOTE: the file needs to be an image file that is < 1MB
            if (imgUri.IsFile)
            {
                var path = imgUri.LocalPath;

                var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                var sizeMb = fs.Length / 1024.0 / 1024.0;

                if (sizeMb > 1.0) // Enforces file size restriction
                    throw new ApplicationException(
                        $"The file {imgUri.LocalPath} is greater than 1MB. Please resize it and try again");

                var sc = new StreamContent(fs);
                mfdc.Add(
                    sc, // binay image path
                    "image", // name = image
                    "image" // filename = image
                );
            }

            // Part #3 - Add KnowledgeRequest JSON object
            mfdc.Add(new StringContent(JsonConvert.SerializeObject(request)), "knowledgeRequest");

            // Part #4 - Invoke the service and read the response
            var response = client.PostAsync(endpointUrl, mfdc);

            // Part # 5 - Do what you like with the data
            Console.WriteLine(response.Result.Content.ReadAsStringAsync().Result);
            File.WriteAllText(@"response.txt", response.Result.Content.ReadAsStringAsync().Result);
            Console.ReadLine();
        }
    }
}