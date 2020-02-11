using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace core.helpers
{
    public static class JsonSerializerWrapper
    {
        private static readonly JsonSerializerSettings Options;

        static JsonSerializerWrapper()
        {
            Options = new JsonSerializerSettings();
            Options.Converters.Add(new StringEnumConverter());
            Options.NullValueHandling = NullValueHandling.Ignore;
        }

        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Options);
        }

        public static T Deserialize<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString, Options);
        }
    }
}