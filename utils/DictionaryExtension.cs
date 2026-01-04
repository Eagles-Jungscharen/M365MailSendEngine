namespace EaglesJungscharen.Azure.Mailsender.Extension;

public static class DictionaryExtension {
    public static string GetString(this IDictionary<string, object> values, string key)
    {
        return values.TryGetValue(key, out object? value) ? Convert.ToString(value) ?? "" : "";
    }
    public static decimal GetDecimal(this IDictionary<string, object> values, string key)
    {
        return values.TryGetValue(key, out object? value) ? Convert.ToDecimal(value) : 0;
    }
    public static bool GetBool(this IDictionary<string, object> values, string key)
    {
        return values.TryGetValue(key, out object? value) ? Convert.ToBoolean(value) : false;
    }

}