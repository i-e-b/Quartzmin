#nullable enable
using Quartzmin.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HttpRequest = Microsoft.AspNetCore.Http.HttpRequest;

namespace Quartzmin.Helpers;

internal static class JobDataMapRequest
{
    public static Task<Dictionary<string, object>[]> GetJobDataMapForm(this IEnumerable<KeyValuePair<string, object>> formData, bool includeRowIndex = true)
    {
        // key1 is group, key2 is field
        var map = new Dictionary<string, Dictionary<string, object>>();

        foreach (var item in formData)
        {
            if (item.Key is null) continue;
            var g = GetJobDataMapFieldGroup(item.Key);
            if (g != null)
            {
                var field = item.Key.Substring(0, item.Key.Length - g.Length - 1);
                if (!map.ContainsKey(g))
                    map[g] = new Dictionary<string, object>();
                map[g]![field] = item.Value;
            }
        }

        if (includeRowIndex)
        {
            foreach (var g in map.Keys)
                map[g]!["data-map[index]"] = g;
        }

        return Task.FromResult(map.Values.ToArray());
    }

    public static async Task<Dictionary<string, object>[]> GetJobDataMapForm(this HttpRequest request, bool includeRowIndex = true)
    {
        return await GetJobDataMapForm(await request.GetFormData(), includeRowIndex);
    }

    private static string? GetJobDataMapFieldGroup(string field)
    {
        var n = field.LastIndexOf(':');
        if (n == -1) return null;

        return field.Substring(n + 1);
    }

    public static Task<List<KeyValuePair<string, object>>> GetFormData(this HttpRequest request)
    {
        var result = new List<KeyValuePair<string, object>>();

        var keys = request.Form?.Keys.ToListOrEmpty()!;
        foreach (var key in keys)
        {
            foreach (var strValue in request.Form?[key].ToListOrEmpty()!)
                result.Add(new KeyValuePair<string, object>(key, strValue));
        }
        foreach (var file in request.Form?.Files.ToListOrEmpty()!)
            result.Add(new KeyValuePair<string, object>(file.Name, new FormFile(file)));

        return Task.FromResult(result);
    }
}