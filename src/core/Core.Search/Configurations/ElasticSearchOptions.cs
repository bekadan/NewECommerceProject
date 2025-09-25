namespace Core.Search.Configurations;

public class ElasticSearchOptions
{
    public string Url { get; set; } = string.Empty;
    public string DefaultIndex { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
