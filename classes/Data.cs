using LunaLib.Json;

static class Data
{
    public static Dictionary<string, object> Json = [];

    public static void Save()
    {
        Serializer.Save(Json, "data/data.json");
    }
}