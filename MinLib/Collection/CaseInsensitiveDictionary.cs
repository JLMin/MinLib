
using MinLib.Extension;
using System.Diagnostics.CodeAnalysis;

namespace MinLib.Collection;

public class CaseInsensitiveDictionary<TValue> : Dictionary<string, TValue>
{
    public CaseInsensitiveDictionary()
    : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    public CaseInsensitiveDictionary(IDictionary<string, TValue> dictionary)
    : base(dictionary, StringComparer.OrdinalIgnoreCase)
    {
    }

    public CaseInsensitiveDictionary(IEnumerable<KeyValuePair<string, TValue>> collection)
    : base(collection, StringComparer.OrdinalIgnoreCase)
    {
    }

    public bool TryGetKey(string key, [MaybeNullWhen(false)] out string actualKey)
    {
        actualKey = Keys.Where(k => k.EqualsIgnoreCase(key)).FirstOrDefault();
        return actualKey != null;
    }

    public bool TryRemove(string key, [MaybeNullWhen(false)] out TValue value)
    {
        value = default;
        if (ContainsKey(key))
        {
            TryGetValue(key, out value);
            Remove(key);
        }
        return value != null;
    }

    public CaseInsensitiveDictionary<TValue> FilterKey(Func<string, bool> condition)
    {
        CaseInsensitiveDictionary<TValue>? newDict = new CaseInsensitiveDictionary<TValue>();
        IEnumerable<KeyValuePair<string, TValue>>? pairs = this.Where(n => condition(n.Key));
        foreach (KeyValuePair<string, TValue> pair in pairs)
        {
            newDict[pair.Key] = pair.Value;
        }
        return newDict;
    }
}
