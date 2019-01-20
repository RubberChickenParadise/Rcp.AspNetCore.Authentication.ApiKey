// Copyright (c) 2019 Jeremy Oursler All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExampleAndTestApp.Controllers
{
    [Route("api/values")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        [Route("anon")]
        public ActionResult<IEnumerable<string>> GetAnon()
        {
            return new[] {"value1", "value2"};
        }

        // GET api/values
        [HttpGet]
        [Authorize]
        [Route("generalauthorize")]
        public ActionResult<IEnumerable<string>> GetGeneralAuthorize()
        {
            return new[] {"value1", "value2"};
        }

        // GET api/values
        [HttpGet]
        [Authorize(Policy = "TestPolicy")]
        [Route("policyauthorize")]
        public ActionResult<IEnumerable<string>> GetPolicyAuthorize()
        {
            return new[] {"value1", "value2"};
        }
    }
}
