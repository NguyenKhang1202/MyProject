using Microsoft.Extensions.Options;
using MyProject.Domain;
using MyProject.Domain.Elasticsearchs;
using Nest;

namespace MyProject.Services;

public class ElasticSearchService
{
    private readonly ElasticClient _client;

    public ElasticSearchService(IOptions<ElasticsearchSettings> options)
    {
        var settings1 = options.Value;
        var settings = new ConnectionSettings(new Uri(settings1.Url))
            .DefaultIndex(settings1.DefaultIndex)
            .DisableDirectStreaming()
            .ThrowExceptions(alwaysThrow: true)
            .PrettyJson();
        _client = new ElasticClient(settings);
    }

    public IndexResponse IndexDocument(UserElastic user)
    {
        return _client.IndexDocument(user);
    }

    public ISearchResponse<UserElastic> SearchUser(string name)
    {
        var result = _client.Search<UserElastic>(s => s
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Name)
                    .Query(name)
                )
            )
        );
        return result;
    }
}
