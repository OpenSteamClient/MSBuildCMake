namespace CustomBuildTask;

public static class RIDParser {
    public static (string os, string arch) Parse(string ridstring)
    {
        //TODO: this logic could probably be improved
        if (!ridstring.Contains('-')) {
            throw new ArgumentException("Invalid RID " + ridstring, nameof(ridstring));
        }

        return (ridstring.Split('-')[0], ridstring.Split('-')[1]);
    }
}