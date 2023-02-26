using System;
using System.Collections.Generic;
namespace Eagels.MailSender.Extension;

public static class DictionaryExtension {
    public static string GetString(this IDictionary<string, object> values, string key)
    {
        return values.ContainsKey(key) ? Convert.ToString(values[key]) : "";
    }
    public static decimal GetDecimal(this IDictionary<string, object> values, string key)
    {
        return values.ContainsKey(key) ? Convert.ToDecimal(values[key]) : 0;
    }
    public static bool GetBool(this IDictionary<string, object> values, string key)
    {
        return values.ContainsKey(key) ? Convert.ToBoolean(values[key]) : false;
    }

}