using System;
using System.Collections.Generic;
using System.Web;
using Nest;
using Nest.Indexify;
using Nest.Indexify.Contributors.Analysis.English;
using Nest.Indexify.Contributors.IndexSettings;
using Umbraco.Core;
using Umbraco.Elasticsearch.Core;
using Umbraco.Elasticsearch.Core.Config;
using Umbraco.Elasticsearch.Core.Content;
using Umbraco.Elasticsearch.Core.EventHandlers;
using Umbraco.Elasticsearch.Core.Media;
using Umbraco.Elasticsearch.Samplev73.Features.Search.Services.Article;
using Umbraco.Elasticsearch.Samplev73.Features.Search.Services.Image;
using Umbraco.Web;

namespace Umbraco.Elasticsearch.Samplev73.Features.Search
{
    public class UmbracoElasticsearchStartup : SearchApplicationEventHandler
    {
        public UmbracoElasticsearchStartup()
        {
            SearchifyMvcConfig.Configure(HttpContext.Current.ApplicationInstance);
        }

        protected override IElasticClient ConfigureElasticClient(FromConfigSearchSettings searchSettings)
        {
            var indexResolver = new DefaultIndexNameResolver();
            var indexName = indexResolver.Resolve(searchSettings, searchSettings.IndexName);
            var connection = new ConnectionSettings(new Uri(searchSettings.Host), indexName);
            connection.EnableTrace();
            connection.ExposeRawResponse();
            connection.PrettyJson();
            return new ElasticClient(connection);
        }

        protected override IElasticsearchIndexCreationStrategy GetIndexCreationStrategy(IElasticClient client)
        {
            return new UmbracoElasticsearchIndexCreationStrategy(client);
        }

        protected override IEnumerable<IContentIndexService> RegisterContentIndexingServices()
        {
            yield return new ArticleContentIndexService(UmbracoSearchFactory.Client, UmbracoContext.Current);
        }

        protected override IEnumerable<IMediaIndexService> RegisterMediaIndexingServices()
        {
            yield return new ImageMediaIndexService(UmbracoSearchFactory.Client, UmbracoContext.Current);
        }

        internal class UmbracoElasticsearchIndexCreationStrategy : ElasticsearchIndexCreationStrategy
        {
            public UmbracoElasticsearchIndexCreationStrategy(IElasticClient client) : base(client)
            {
                AddContributor(new EnglishIndexAnalysisContributor());
                AddContributor(new IndexSettingsContributor(1, 1));
            }
        }
    }

}