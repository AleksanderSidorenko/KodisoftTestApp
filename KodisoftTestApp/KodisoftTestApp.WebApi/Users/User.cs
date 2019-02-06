using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace TasksWebApi.Controllers.Users
{
    public class User
    {
        [BsonId]
        public Guid Id { get; set; }
        [BsonElement("userName")]
        public string Username { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }
        [BsonElement("token")]
        public string Token { get; set; }
    }
}