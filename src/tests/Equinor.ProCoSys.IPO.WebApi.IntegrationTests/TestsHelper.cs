using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public static class TestsHelper
    {
        public static async Task AssertResponseAsync(
            HttpResponseMessage response, 
            HttpStatusCode expectedStatusCode,
            string expectedMessagePartOnBadRequest)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Bad request details: {jsonString}");
                
                if (!string.IsNullOrEmpty(expectedMessagePartOnBadRequest))
                {
                    var problemDetails = JsonConvert.DeserializeObject<ValidationProblemDetails>(jsonString);
                    Assert.IsTrue(problemDetails.Errors.SelectMany(e => e.Value).Any(e => e.Contains(expectedMessagePartOnBadRequest)));
                }
            }

            Assert.AreEqual(expectedStatusCode, response.StatusCode);
        }

        public static async Task AssertInternalServerErrorAsync(
            HttpResponseMessage response, 
            HttpStatusCode expectedStatusCode, 
            string expectedMessageOnInternalServerError)
        {
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"server error details: {jsonString}");
                Assert.AreEqual(expectedMessageOnInternalServerError, jsonString);
            }

            Assert.AreEqual(expectedStatusCode, response.StatusCode);
        }
    }
}
