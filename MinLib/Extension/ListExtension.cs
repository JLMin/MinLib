namespace MinLib.Extension;

public static class ListExtension
{
    public static List<V> Filter<V>(this List<V> list, Func<V, bool> condition)
    {
        return list.Where(v => condition(v)).ToList();
    }
}
