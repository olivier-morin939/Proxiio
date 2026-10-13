using Entities;

namespace RepositoryContracts
{
    /// <summary>
    /// Lightweight projection used by the community feed.
    /// Only the columns needed from the user and the community are loaded,
    /// instead of the full User and Community entities.
    /// </summary>
    public sealed class PostFeedItem
    {
        public Post Post { get; init; } = null!;

        public string? AuthorName { get; init; }

        public Guid CommunityId { get; init; }

        public string? CommunityName { get; init; }

        public string? CommunityDescription { get; init; }
    }
}
