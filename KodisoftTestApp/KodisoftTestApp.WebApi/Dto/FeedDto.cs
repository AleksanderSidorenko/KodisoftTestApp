using MongoDB.Bson.Serialization.Attributes;
using System;

namespace KodisoftTestApp.WebApi.Dto
{
    public class FeedDto
    {
        [BsonId]
        public Guid Id { get; set; }

        [BsonElement("collectionId")]
        public Guid CollectionId { get; set; }

        [BsonElement("summary")]
        public string Summary { get; set; }

        [BsonElement("text")]
        public string Text { get; set; }

    }
}
