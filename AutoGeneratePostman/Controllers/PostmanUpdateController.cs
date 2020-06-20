using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Newtonsoft.Json;

namespace AutoGeneratePostman.Controllers
{
    /// <summary>
    /// Update Postman Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PostmanUpdateController : ControllerBase
    {

        private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;

        /// <summary>
        /// api for update postman
        /// </summary>
        /// <param name="apiExplorer"></param>
        public PostmanUpdateController(IApiDescriptionGroupCollectionProvider apiExplorer)
        {
            _apiExplorer = apiExplorer;
        }

        // GET: api/PostmanUpdate
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", "PMAK-5e70636401907e0030e83267-3d5cfe48d6a3a0296c408cadc3811606b3");

            var postmanHelper = new PostmanHelper(_apiExplorer);
            var collection = postmanHelper.GetPostmanCollection();

            var collectionItemDtos = collection.Folders.Select(x =>
            {
                var collectionItemDto = new CollectionItemDto(x.Name,
                    collection.Requests.Where(c => x.Order.Contains(c.Id)).ToList().Select(n =>
                    {
                        var httpHeaderDto = new HttpHeaderDto("Content-Type", "application/json");
                        var httpMethod = new HttpMethod(n.Method);
                        var notGetMethod = httpMethod != HttpMethod.Get;
                        var postmanRequestDto = new PostmanRequestDto(httpMethod,
                            notGetMethod ?
                                new List<HttpHeaderDto>()
                                {
                                    httpHeaderDto
                                } : null, notGetMethod ? new BodyDto(n.DataMode, n.Data) : null, n.Url,
                            n.Description);
                        var itemDto = new ItemDto(n.Name,
                            postmanRequestDto);
                        return itemDto;
                    }).ToList());
                return collectionItemDto;
            }).ToList();

            var collectionInfo = new CollectionInfoDto(
                Guid.Parse("1ed5a7b1-bbc8-48ca-8535-36c4cc1169a9"),
                collection.Name, collection.Description,
                "https://schema.getpostman.com/json/collection/v2.1.0/collection.json");
            var postmanDto = new PostmanDto()
            {
                collection = new CollectionDto(collectionInfo,
                    collectionItemDtos)
            };
            const string uid = "1958241-1ed5a7b1-bbc8-48ca-8535-36c4cc1169a9";
            var requestUri = new Uri($"https://api.getpostman.com/collections/{uid}");

            var serializeObject = JsonConvert.SerializeObject(postmanDto, Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            //serializeObject = serializeObject.ToLower();
            var httpResponseMessage = await client.PutAsync(requestUri, new StringContent(serializeObject));

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return Ok("ویرایش پست من با موفقیت انجام شد");
            }
            var readAsStringAsync = await httpResponseMessage.Content.ReadAsStringAsync();
            return BadRequest(readAsStringAsync);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PostmanDto
    {
        /// <summary>
        /// 
        /// </summary>
        public CollectionDto collection { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CollectionDto
    {
        public CollectionDto(CollectionInfoDto info, List<CollectionItemDto> item)
        {
            this.info = info;
            this.item = item;
        }

        /// <summary>
        /// اطلاعات کالکشن
        /// </summary>
        public CollectionInfoDto info { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<CollectionItemDto> item { get; set; }
    }

    public class CollectionItemDto
    {
        public CollectionItemDto(string name, List<ItemDto> item)
        {
            this.name = name;
            this.item = item;
        }

        /// <summary>
        /// نام 
        /// </summary>
        public string name { get; set; }

        public List<ItemDto> item { get; set; }

    }

    public class ItemDto
    {
        public ItemDto(string name, PostmanRequestDto request)
        {
            this.name = name;
            this.request = request;
        }

        /// <summary>
        /// نام 
        /// </summary>
        public string name { get; set; }

        public PostmanRequestDto request { get; set; }
    }

    public class PostmanRequestDto
    {
        public PostmanRequestDto(HttpMethod Method, List<HttpHeaderDto> header, BodyDto body, string url, string description)
        {
            method = Method.Method;
            this.header = header;
            this.body = body;
            this.url = url;
            this.description = description;
        }

        public string method { get; set; }
        public List<HttpHeaderDto> header { get; set; }
        public BodyDto body { get; set; }
        public string url { get; set; }
        public string description { get; set; }

    }

    public class BodyDto
    {
        public BodyDto(string mode, string raw)
        {
            this.mode = mode;
            this.raw = raw;
        }

        public string mode { get; set; }
        public string raw { get; set; }
        //public BodyOptions Options { get; set; }

    }

    public class HttpHeaderDto
    {
        public HttpHeaderDto(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public string key { get; set; }
        public string value { get; set; }
    }

    public class CollectionInfoDto
    {
        public CollectionInfoDto(Guid postmanId, string name, string description, string schema)
        {
            _postman_id = postmanId;
            this.name = name;
            this.description = description;
            this.schema = schema;
        }

        /// <summary>
        /// شناسه پست من
        /// </summary>
        public Guid _postman_id { get; set; }
        /// <summary>
        /// نام 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// توضیحات
        /// </summary>
        public string description { get; set; }
        public string schema { get; set; }


    }
}
