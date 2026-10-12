using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LunaLib.Json;

public static class Serializer // boring data :v
{
    public static void Save(Dictionary<string, object> data, string path)
    {
        string ? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public static Dictionary<string, object> Load(string path)
    {
        if (!File.Exists(path))
            return [];

        try
        {
            string json = File.ReadAllText(path);

            var raw = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return raw != null ? ConvertTokens(raw) : [];
        }
        catch
        {
            return [];
        }
    }

    private static Dictionary<string, object> ConvertTokens(Dictionary<string, object> dict)
    {
        var result = new Dictionary<string, object>();

        foreach (var kv in dict)
        {
            result[kv.Key] = ConvertToken(kv.Value);
        }

        return result;
    }

    private static object ConvertToken(object value)
    {
        return value switch
        {
            JObject obj => ConvertTokens(obj.ToObject<Dictionary<string, object>>()!),
            JArray arr => arr.Select(ConvertToken).ToList(),
            _ => value
        };
    }
}
