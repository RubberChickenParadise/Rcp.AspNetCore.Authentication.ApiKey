// Copyright (c) 2019 Jeremy Oursler All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Net;
using System.Threading.Tasks;
using ExampleAndTestApp;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rcp.AspNetCore.Authentication.ApiKey.Test
{
    [TestClass]
    public class ApiKeyAuthenticationHandlerTests
    {
        private TestServer _api;

        [TestCleanup]
        public void Cleanup()
        {
            _api.Dispose();
        }

        [TestInitialize]
        public void Initialize()
        {
            _api = new TestServer(WebHost
                                  .CreateDefaultBuilder()
                                  .UseStartup<Startup>());
        }

        [TestMethod]
        public async Task KeyHeader_GeneralAuth__anon()
        {
            var response = await _api.CreateRequest("/api/values/anon")
                                     .AddHeader("X-API-KEY",
                                                "GeneralAuth")
                                     .GetAsync();

            Assert.AreEqual(HttpStatusCode.OK,
                            response.StatusCode);
        }

        [TestMethod]
        public async Task KeyHeader_GeneralAuth_generalauthorize()
        {
            var response = await _api.CreateRequest("/api/values/generalauthorize")
                                     .AddHeader("X-API-KEY",
                                                "GeneralAuth")
                                     .GetAsync();

            Assert.AreEqual(HttpStatusCode.OK,
                            response.StatusCode);
        }

        [TestMethod]
        public async Task KeyHeader_GeneralAuth_policyauthorize()
        {
            var response = await _api.CreateRequest("/api/values/policyauthorize")
                                     .AddHeader("X-API-KEY",
                                                "GeneralAuth")
                                     .GetAsync();

            Assert.AreEqual(HttpStatusCode.Forbidden,
                            response.StatusCode);
        }

        [TestMethod]
        public async Task KeyHeader_SpecificClaim__anon()
        {
            var response = await _api.CreateRequest("/api/values/anon")
                                     .AddHeader("X-API-KEY",
                                                "SpecificClaim")
                                     .GetAsync();

            Assert.AreEqual(HttpStatusCode.OK,
                            response.StatusCode);
        }

        [TestMethod]
        public async Task KeyHeader_SpecificClaim_generalauthorize()
        {
            var response = await _api.CreateRequest("/api/values/generalauthorize")
                                     .AddHeader("X-API-KEY",
                                                "SpecificClaim")
                                     .GetAsync();

            Assert.AreEqual(HttpStatusCode.OK,
                            response.StatusCode);
        }

        [TestMethod]
        public async Task KeyHeader_SpecificClaim_policyauthorize()
        {
            var response = await _api.CreateRequest("/api/values/policyauthorize")
                                     .AddHeader("X-API-KEY",
                                                "SpecificClaim")
                                     .GetAsync();

            Assert.AreEqual(HttpStatusCode.OK,
                            response.StatusCode);
        }

        [TestMethod]
        public async Task NoAuthHeader_anon()
        {
            var response = await _api.CreateRequest("/api/values/anon")
                                     .GetAsync();

            Assert.AreEqual(HttpStatusCode.OK,
                            response.StatusCode);
        }

        [TestMethod]
        public async Task NoAuthHeader_generalauthorize()
        {
            var response = await _api.CreateRequest("/api/values/generalauthorize")
                                     .GetAsync();

            Assert.AreEqual(HttpStatusCode.Unauthorized,
                            response.StatusCode);
        }

        [TestMethod]
        public async Task NoAuthHeader_policyauthorize()
        {
            var response = await _api.CreateRequest("/api/values/policyauthorize")
                                     .GetAsync();

            Assert.AreEqual(HttpStatusCode.Unauthorized,
                            response.StatusCode);
        }
    }
}
