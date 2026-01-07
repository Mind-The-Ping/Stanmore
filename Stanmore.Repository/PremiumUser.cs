using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Stanmore.Repository;

public class PremiumUser
{
    [property: BsonId]
    [property: BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }
    public DateTime PremiumExpiresAt { get; set; }
    public bool CleanupCompleted { get; set; }
    public DateTime? CleanupCompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}