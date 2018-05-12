using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OSSUploader
{
    public class SignatureGenerator
    {
        private readonly string _accessKeyId;
        private readonly string _keySecret;
        private readonly string _host;
        private readonly string _callbackUrl;

        private string _callBackBody = null;

        public SignatureGenerator(string accessKeyId, string keySecret, string host, string callbackUrl)
        {
            _accessKeyId = accessKeyId;
            _keySecret = keySecret;
            _host = host;
            _callbackUrl = callbackUrl;
        }

        public object Generate(string dir)
        {
            var now = DateTime.UtcNow;
            var end = now.AddSeconds(30);
            var expiration = end.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            var conditions = new List<object[]>(2)
            {
                new object[] { "content-length-range", 0, 10 * 1024 * 1024 },
                new object[] { "starts-with", "$key", dir }
            };

            var policy = new Dictionary<string, object>
            {
                ["expiration"] = expiration,
                ["conditions"] = conditions
            };

            var base64Policy = Base64Json(policy);
            var signatureBytes = HMACSHA1(base64Policy, _keySecret);
            var signature = Base64String(signatureBytes);

            return new
            {
                accessid = _accessKeyId,
                host = _host,
                policy = base64Policy,
                signature,
                expire = end,
                dir,
                callback = GenerateCallback()
            };
        }

        private string GenerateCallback()
        {
            if (string.IsNullOrWhiteSpace(_callBackBody))
            {
                var obj = new OSSCallback
                {
                    Url = _callbackUrl,
                    Body = "filename=${object}&size=${size}&mimeType=${mimeType}&height=${imageInfo.height}&width=${imageInfo.width}",
                    BodyType = "application/x-www-form-urlencoded"
                };

                _callBackBody = Base64Json(obj);
            }

            return _callBackBody;
        }

        private static byte[] HMACSHA1(string signatureString, string secretKey)
        {
            var sh1 = new HMACSHA1(UTF8Bytes(secretKey));
            sh1.Initialize();

            var buffer = UTF8Bytes(signatureString);
            return sh1.ComputeHash(buffer);
        }

        private static string Base64Json<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return Base64String(json);
        }

        private static string Base64String(string str)
        {
            var bytes = UTF8Bytes(str);

            return Base64String(bytes);
        }

        private static byte[] UTF8Bytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        private static string Base64String(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
    }
}
