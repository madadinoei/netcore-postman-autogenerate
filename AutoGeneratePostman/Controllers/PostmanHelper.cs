using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;

namespace AutoGeneratePostman.Controllers
{
    /// <summary>
    /// Postman Helper
    /// </summary>
    public class PostmanHelper
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;

        private readonly Regex _pathVariableRegEx =
            new Regex("\\{([A-Za-z0-9-_]+)\\}", RegexOptions.ECMAScript | RegexOptions.Compiled);

        private readonly Regex _urlParameterVariableRegEx =
            new Regex("=\\{([A-Za-z0-9-_]+)\\}", RegexOptions.ECMAScript | RegexOptions.Compiled);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiExplorer"></param>
        public PostmanHelper(IApiDescriptionGroupCollectionProvider apiExplorer)
        {
            _apiExplorer = apiExplorer;
        }

        public PostmanCollectionGet GetPostmanCollection()
        {
            var baseUri = "{{host}}";

            var postManCollection = new PostmanCollectionGet
            {
                Id = Guid.Parse("1ed5a7b1-bbc8-48ca-8535-36c4cc1169a9"),
                Name = "BimePostmanCollection",
                Timestamp = DateTime.Now.Ticks,
                Requests = new Collection<PostmanRequestGet>(),
                Folders = new Collection<PostmanFolderGet>(),
                Synced = false,
                Description = "Genereated PostmanCollection"
            };


            var helpPageSampleGenerator = new HelpPageSampleGenerator();
            var apiDescriptionsByController = _apiExplorer.ApiDescriptionGroups;


            foreach (var apiDescriptionsByControllerGroup in apiDescriptionsByController.Items)
            {
                var controllerName = apiDescriptionsByControllerGroup.GroupName ?? string.Empty;

                var postManFolder = new PostmanFolderGet
                {
                    Id = Guid.NewGuid(),
                    CollectionId = postManCollection.Id,
                    Name = controllerName,
                    Description = $"Api Methods for {controllerName}",
                    CollectionName = "api",
                    Order = new Collection<Guid>()
                };

                foreach (var apiDescription in apiDescriptionsByControllerGroup.Items)
                {
                    TextSample sampleData = null;
                    var sampleDictionary = helpPageSampleGenerator.GetSample(apiDescription, SampleDirection.Request);
                    if (MediaTypeHeaderValue.TryParse("application/json", out var mediaTypeHeader)
                        && sampleDictionary.ContainsKey(mediaTypeHeader))
                    {
                        sampleData = sampleDictionary[mediaTypeHeader] as TextSample;
                    }

                    // scrub curly braces from url parameter values
                    var cleanedUrlParameterUrl =
                        this._urlParameterVariableRegEx.Replace(apiDescription.RelativePath, "=$1-value");

                    // get pat variables from url
                    var pathVariables = this._pathVariableRegEx.Matches(cleanedUrlParameterUrl)
                        .Select(m => m.Value)
                        .Select(s => s.Substring(1, s.Length - 2))
                        .ToDictionary(s => s, s => $"{s}-value");

                    // change format of parameters within string to be colon prefixed rather than curly brace wrapped
                    var postmanReadyUrl = this._pathVariableRegEx.Replace(cleanedUrlParameterUrl, ":$1");

                    // prefix url with base uri
                    var url = baseUri.TrimEnd('/') + "/" + postmanReadyUrl;

                    var list = apiDescription.ParameterDescriptions.Where(x => x.Source == BindingSource.Query)
                        .ToList();
                    ////var queryParams = string.Join("/", list);
                    ////Multiple Parameters
                    var queryParams1 = new Dictionary<string, string>();

                    foreach (var p in list)
                    {
                        queryParams1.Add(p.Name, p.DefaultValue != null ? p.DefaultValue.ToString() : string.Empty);
                    }
                    ////returns /api/product/list?cat=221&gender=boy&age=4,5,6
                    url = QueryHelpers.AddQueryString(url, queryParams1);
                    var methodInfo = (apiDescription.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
                    var desc = methodInfo.GetXmlDocumentation();
                    var parameterInfos = methodInfo.GetParameters().ToList();
                    foreach (var parameterInfo in parameterInfos)
                    {
                        var x = parameterInfo.GetXmlDocumentation();
                        desc += "\n" + x;
                    }
                    var request = new PostmanRequestGet
                    {
                        CollectionId = postManCollection.Id,
                        Id = Guid.NewGuid(),
                        Name = methodInfo.Name,
                        Description = desc,
                        Url = url,
                        Method = apiDescription.HttpMethod,
                        Headers = "Content-Type: application/json",
                        Data = sampleData?.Text,
                        DataMode = "raw",
                        Time = postManCollection.Timestamp,
                        Synced = false,
                        DescriptionFormat = "markdown",
                        Version = "beta",
                        Responses = new Collection<string>(),
                        PathVariables = pathVariables
                    };

                    postManFolder.Order.Add(request.Id); // add to the folder
                    postManCollection.Requests.Add(request);
                }

                postManCollection.Folders.Add(postManFolder);
            }

            return postManCollection;
        }
    }
}