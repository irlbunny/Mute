using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Linq;

namespace Mute.Providers
{
    internal static class TokenFetcher
    {
        public class LocalizedResources
        {
            [JsonProperty("token")]
            public string Token { get; set; }
            [JsonProperty("region")]
            public string Region { get; set; }
        }

        public static LocalizedResources GetResources()
        {
            var document = new HtmlWeb().Load("https://azure.microsoft.com/en-us/services/cognitive-services/speech-to-text");
            var nodes = document.DocumentNode.SelectNodes("//script").ToArray();
            foreach (var node in nodes)
            {
                if (node.InnerHtml.Contains("localizedResources"))
                {
                    var json = node.InnerHtml;
                    json = json.Replace("var localizedResources = ", string.Empty);
                    json = json.Split('}')[0] + "}";
                    return JsonConvert.DeserializeObject<LocalizedResources>(json);
                }
            }
            return null;
        }
    }
}
