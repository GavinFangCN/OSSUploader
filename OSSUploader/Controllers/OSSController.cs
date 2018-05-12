using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OSSUploader.Controllers
{
    [Route("api/[controller]")]
    public class OSSController : Controller
    {
        // GET api/values
        [HttpGet]
        public object Get()
        {
            var id = "6MKOqxGiGU4AUk44";
            var key = "ufu7nS8kS59awNihtjSonMETLI0KLy";
            var host = "http://post-test.oss-cn-hangzhou.aliyuncs.com";
            var callbackUrl = "http://oss-demo.aliyuncs.com:23450";

            var generator = new SignatureGenerator(id, key, host, callbackUrl);
            var obj = generator.Generate("test");

            return obj;
        }

        [HttpGet("complete")]
        public void Complete()
        {
            var request = HttpContext.Request;

            Console.WriteLine(request.QueryString);
        }
    }
}
