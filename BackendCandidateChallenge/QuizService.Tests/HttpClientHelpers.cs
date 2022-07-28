using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace QuizService.Tests
{
    internal class HttpClientHelpers
    {
        public static StringContent CreateStringContent(object request)
        {
            var content = new StringContent(JsonConvert.SerializeObject(request));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }
    }
}
