using KodisoftTestApp.WebApi.Dto;
using KodisoftTestApp.WebApi.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace KodisoftTestApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CollectionsController : ControllerBase
    {
        protected IMongoDatabase _database;
        private IMongoCollection<CollectionDto> _collections;
        private IMongoCollection<FeedDto> _feeds;
        private IMemoryCache _cache;
        protected Guid? UserId => User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault() != null ? (Guid?)Guid.Parse(User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value) : null;
        protected string Role => User.Claims.Where(c => c.Type == ClaimTypes.Role).FirstOrDefault() != null ? User.Claims.Where(c => c.Type == ClaimTypes.Role).First().Value : null;

        public CollectionsController(IMongoDatabase database, IMemoryCache memoryCache)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _collections = _database.GetCollection<CollectionDto>("Collections");
            _feeds = _database.GetCollection<FeedDto>("Feeds");
            _cache = memoryCache;
        }

        // GET: api/Collections/5
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(Guid id)
        {
            var collection = _collections.Find(Builders<CollectionDto>.Filter.Eq(c => c.Id, id)).SingleOrDefault();
            if (collection == null)
            {
                return NotFound("Collection is not exist");
            }

            IEnumerable<FeedDto> feeds;

            if (!_cache.TryGetValue(id, out feeds))
            {
                feeds = _feeds.Find(Builders<FeedDto>.Filter.Eq(c => c.CollectionId, id)).ToEnumerable();
        
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(10));

                _cache.Set(id, feeds, cacheEntryOptions);
            }

            return Ok(feeds);
        }

        // POST: api/Collections
        [HttpPost]
        public IActionResult Post([FromBody] CreateCollectionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name.Trim()))
            {
                return BadRequest();
            }

            var collection = new CollectionDto { Id = Guid.NewGuid(), Name = request.Name };
            _collections.InsertOne(collection);
            return Ok(new { collection.Id });
        }

        // PUT: api/Collections/5
        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody] CreateFeedRequest request)
        {
            var collection = _collections.Find(Builders<CollectionDto>.Filter.Eq(c => c.Id, id)).SingleOrDefault();
            if (collection == null)
            {
                return NotFound("Collection is not exist");
            }

            var feed = new FeedDto { Id = Guid.NewGuid(), CollectionId = id, Summary = request.Summary, Text = request.Text };
            _feeds.InsertOne(feed);
            _cache.Remove(id);
            return Ok(feed);
        }
    }
}
