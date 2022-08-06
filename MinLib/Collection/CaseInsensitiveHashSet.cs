namespace MinLib.Collection;

public class CaseInsensitiveHashSet : HashSet<string>
{
    public CaseInsensitiveHashSet()
    : base(StringComparer.OrdinalIgnoreCase)
    {
    }
}
