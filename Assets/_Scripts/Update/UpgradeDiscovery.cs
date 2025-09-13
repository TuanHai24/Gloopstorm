using System.Collections.Generic;

public static class UpgradeDiscovery
{
    private static readonly HashSet<string> discovered = new();

    public static bool IsDiscovered(string id) =>
        !string.IsNullOrEmpty(id) && discovered.Contains(id);

    public static void MarkDiscovered(string id)
    {
        if (!string.IsNullOrEmpty(id)) discovered.Add(id);
    }
}
