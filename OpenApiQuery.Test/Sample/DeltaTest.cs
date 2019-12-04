using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Sample.Models;

namespace OpenApiQuery.Test.Sample
{
    [TestClass]
    public class DeltaTest : SampleTestBase
    {
        [TestMethod]
        public async Task TestPatch_Success()
        {
            using var server = SetupSample(new[]
            {
                new User
                {
                    Username = "Test",
                    FirstName = "A",
                    LastName = "B",
                    EMail = "test@mail.local"
                }
            });
            using var client = server.CreateClient();

            var user = (await client.GetQueryAsync<User>("/users")).ResultItems[0];
            Assert.AreEqual("A", user.FirstName);
            Assert.AreEqual("B", user.LastName);

            using var patchResponse = await client.PatchAsync($"/users/{user.Id}",
                new StringContent(@"{
                ""firstName"": ""Foo"",
                ""lastName"": ""Bar""
            }", Encoding.UTF8, "application/json"));

            var patchResponseText = await patchResponse.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, patchResponse.StatusCode, "Response was: " + patchResponseText);

            var changedUser = (await client.GetQueryAsync<User>("/users/")).ResultItems[0];
            Assert.AreEqual("Foo", changedUser.FirstName);
            Assert.AreEqual("Bar", changedUser.LastName);
        }
    }
}
