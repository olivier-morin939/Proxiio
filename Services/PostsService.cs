using CsvHelper;
using Entities;
using Entities.Enums;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Posts;
using ServiceContracts.DTO.Users;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Services;

public class PostsService : IPostsService
{

    // Estimated size of a CSV row, used to size the MemoryStream once instead of letting it double repeatedly
    private const int EstimatedBytesPerCsvRow = 256;

    private readonly IPostsRepository _repository;
    private readonly ILogger<PostsService>? _logger;
    public PostsService(IPostsRepository Repository, ILogger<PostsService>? logger = null) { _repository = Repository; _logger = logger; }

    private void LogOperation(string method, string operation, string arguments)
    {
        _logger?.LogInformation("[+][{Method}] {Operation} reached.", method, operation);
        _logger?.LogInformation("[+][{Method}] {Operation} received arguments: {Arguments}.", method, operation, arguments);
    }

    public async Task<List<PostResponse>> GetAllPosts()
    {
        LogOperation("GET", nameof(GetAllPosts), "(aucun)");
        return await AddReportCounts(await _repository.GetAllPosts());
    }

    public Task<int> GetAllPostsCount()
    {
        LogOperation("GET", nameof(GetAllPostsCount), "(aucun)");
        return _repository.GetPostCount();
    }

    public async Task<List<FeedPostResponse>> GetCommunityFeed(Guid? CommunityId = null)
    {
        LogOperation("GET", nameof(GetCommunityFeed), $"CommunityId:{CommunityId}");
        // Get the feed items (post + author + community) from the repository
        List<PostFeedItem> Items = await _repository.GetCommunityFeed(CommunityId);

        // Convert them into DTO responses (list sized in advance)
        List<FeedPostResponse> Responses = new List<FeedPostResponse>(Items.Count);
        foreach (PostFeedItem Item in Items)
        {
            Responses.Add(new FeedPostResponse
            {
                Post = Item.Post.ToPostResponse(),
                AuthorName = Item.AuthorName ?? "Membre",
                CommunityName = Item.CommunityName ?? "Communauté",
                CommunityDescription = Item.CommunityDescription,
                CommunityId = Item.CommunityId
            });
        }

        return Responses;
    }

    public async Task<PostResponse> AddPost(AddPostRequest? Request)
    {
        LogOperation("POST", nameof(AddPost), $"Request:{Request?.GetType().Name}, CommunityId:{Request?.ComunityId}, UserId:{Request?.UserId}");

        ArgumentNullException.ThrowIfNull(Request);
        Helpers.HelpersValidation.ModelValidation(Request);

        if (string.IsNullOrWhiteSpace(Request.Title) || string.IsNullOrWhiteSpace(Request.Body))
            throw new ArgumentException("A title and message are required.");

        if (!await _repository.CommunityExists(Request.ComunityId))
            throw new ArgumentException("Community not found.", nameof(Request.ComunityId));

        if (!await _repository.UserExists(Request.UserId))
            throw new ArgumentException("User not found.", nameof(Request.UserId));

        Post NewPost = Request.ToPost();
        Post AddedPost = await _repository.AddPost(NewPost)
            ?? throw new InvalidOperationException("The post could not be added.");

        return AddedPost.ToPostResponse();
    }

    public async Task<PostResponse> GetPostByPostId(Guid PostId)
    {
        LogOperation("GET", nameof(GetPostByPostId), $"PostId:{PostId}");
        Post? FoundPost = await _repository.GetPostByPostId(PostId);

        if (FoundPost is null)
            throw new KeyNotFoundException($"Post {PostId} was not found.");

        return FoundPost.ToPostResponse();
    }

    public async Task<PostResponse> UpdatePost(UpdatePostRequest Request)
    {
        LogOperation("POST", nameof(UpdatePost), $"Request:{Request?.GetType().Name}, PostId:{Request?.Id}, CommunityId:{Request?.ComunityId}, UserId:{Request?.UserId}");
        
        ArgumentNullException.ThrowIfNull(Request);
        Helpers.HelpersValidation.ModelValidation(Request);


        Post? PostToUpdate = await _repository.GetPostByPostId(Request.Id);

        if (PostToUpdate is null)
            throw new KeyNotFoundException($"Post {Request.Id} was not found.");

        if (string.IsNullOrWhiteSpace(Request.Title) || string.IsNullOrWhiteSpace(Request.Body))
            throw new ArgumentException("A title and message are required.");


        Post actualRequest = Request.ToPost();

        PostToUpdate.Title = actualRequest.Title;
        PostToUpdate.Body = actualRequest.Body;
        PostToUpdate.ImagesPath = actualRequest.ImagesPath ?? new();
        PostToUpdate.AdditionalsPath = actualRequest.AdditionalsPath ?? new();
        PostToUpdate.ModifiedAt = actualRequest.ModifiedAt;

        Post UpdatedPost = await _repository.UpdatePost(PostToUpdate)
            ?? throw new InvalidOperationException("The post could not be updated.");
        return UpdatedPost.ToPostResponse();
    }

    public async Task<bool> DeletePostByPostId(Guid PostId)
    {
        LogOperation("POST", nameof(DeletePostByPostId), $"PostId:{PostId}");
        int RowsDeleted = await _repository.DeletePostByPostId(PostId);
        return RowsDeleted > 0;
    }

    public async Task<List<PostResponse>> GetAllPostsByComunity(Guid ComId)
    {
        LogOperation("GET", nameof(GetAllPostsByComunity), $"CommunityId:{ComId}");
        // Checking if the community id is valid
        EnsureCommunityIdIsValid(ComId);

        // Give the numbers of reports per posts of the given community id
        return await AddReportCounts(await _repository.GetAllPostsByComunity(ComId));
    }

    public async Task<List<PostResponse>> GetAllFilteredPostsByComunitiy(Guid ComId, string SearchBy, string SearchString)
    {
        LogOperation("GET", nameof(GetAllFilteredPostsByComunitiy), $"CommunityId:{ComId}, SearchBy:{SearchBy}, SearchString:{SearchString}");
        // Checking if the community id is valid
        EnsureCommunityIdIsValid(ComId);

        // Checking if the searchBy and searchString is null or empty
        if (string.IsNullOrEmpty(SearchBy) || string.IsNullOrEmpty(SearchString))
            return await GetAllPostsByComunity(ComId);

        // The creation date is searched on its formatted text, which SQL cannot translate: filtered in memory
        // (reports are counted only for the matching posts)
        if (SearchBy == nameof(PostResponse.CreatedAt))
        {
            List<Post> CommunityPosts = await _repository.GetAllPostsByComunity(ComId);
            List<Post> MatchingPosts = CommunityPosts.FindAll(p =>
                p.CreatedAt.ToString("dd MMM yyyy").Contains(SearchString, StringComparison.OrdinalIgnoreCase));

            return await AddReportCounts(MatchingPosts);
        }

        // The other fields are filtered by the database, so only the matching posts are loaded
        Expression<Func<Post, bool>> Predicate = BuildSearchPredicate(ComId, SearchBy, SearchString.ToLower());
        return await AddReportCounts(await _repository.GetAllFilteredPostsByComunitiy(Predicate));
    }

    public Task<List<PostResponse>> GetAllSortedPostsByComunity(Guid ComId, List<PostResponse> Posts, string SortBy, SortOption SortOrder)
    {
        LogOperation("GET", nameof(GetAllSortedPostsByComunity), $"CommunityId:{ComId}, PostsCount:{Posts?.Count}, SortBy:{SortBy}, SortOrder:{SortOrder}");
        Posts ??= new List<PostResponse>();

        // Checking if the sortBy is null or empty
        if (string.IsNullOrEmpty(SortBy))
            return Task.FromResult(Posts);

        // Checking if the order is descending
        bool IsDescending = SortOrder != SortOption.ASC;

        // Sort based on the sortBy fields and sort order
        List<PostResponse> SortedPosts = SortBy switch
        {
            nameof(PostResponse.UserId) => IsDescending ? Posts.OrderByDescending(p => p.UserId).ToList() : Posts.OrderBy(p => p.UserId).ToList(),
            nameof(PostResponse.Title) => IsDescending ? Posts.OrderByDescending(p => p.Title).ToList() : Posts.OrderBy(p => p.Title).ToList(),
            nameof(PostResponse.Body) => IsDescending ? Posts.OrderByDescending(p => p.Body).ToList() : Posts.OrderBy(p => p.Body).ToList(),
            nameof(PostResponse.CreatedAt) => IsDescending ? Posts.OrderByDescending(p => p.CreatedAt).ToList() : Posts.OrderBy(p => p.CreatedAt).ToList(),
            _ => Posts
        };
        return Task.FromResult(SortedPosts);
    }

    public async Task<MemoryStream> GetPostsCSV()
    {
        LogOperation("GET", nameof(GetPostsCSV), "(aucun)");

        List<PostResponse> Posts = await GetAllPosts();

    
        MemoryStream PostsStream = new MemoryStream(Posts.Count * EstimatedBytesPerCsvRow);

        await using (StreamWriter TextWriter = new StreamWriter(PostsStream, new UTF8Encoding(false), 1024, leaveOpen: true))
        await using (CsvWriter CsvOutput = new CsvWriter(TextWriter, CultureInfo.InvariantCulture, leaveOpen: true))
        {
            CsvOutput.WriteHeader<PostResponse>();
            CsvOutput.NextRecord();

            await CsvOutput.WriteRecordsAsync(Posts);
        }

        PostsStream.Position = 0;

        return PostsStream;
    }

    public async Task<MemoryStream> GetPostsEXCEL()
    {
        LogOperation("GET", nameof(GetPostsEXCEL), "(aucun)");
        List<PostResponse> allPosts = (await _repository.GetAllPosts()).Select(post => post.ToPostResponse()).ToList();

        MemoryStream memoryStream = new MemoryStream();

        using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
        {

            ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add("PostsSheet");


            workSheet.Cells["A1"].Value = "Post Id";
            workSheet.Cells["B1"].Value = "Community Id";
            workSheet.Cells["C1"].Value = "User Id";
            workSheet.Cells["D1"].Value = "Title";
            workSheet.Cells["E1"].Value = "Body";
            workSheet.Cells["F1"].Value = "Images count";
            workSheet.Cells["G1"].Value = "Files count";
            workSheet.Cells["H1"].Value = "Created At";
            workSheet.Cells["I1"].Value = "Modified At";

            int row = 2;

            foreach (PostResponse postResponse in allPosts)
            {
                workSheet.Cells[$"A{row}"].Value = postResponse.Id.ToString();
                workSheet.Cells[$"B{row}"].Value = postResponse.CommunityId;
                workSheet.Cells[$"C{row}"].Value = postResponse.UserId;
                workSheet.Cells[$"D{row}"].Value = postResponse.Title;
                workSheet.Cells[$"E{row}"].Value = postResponse.Body;
                workSheet.Cells[$"F{row}"].Value = postResponse?.ImagesPath?.Count > 0 ? postResponse.ImagesPath.Count().ToString() : "No images";
                workSheet.Cells[$"G{row}"].Value = postResponse?.AdditionalsPath?.Count > 0 ? postResponse.AdditionalsPath.Count().ToString() : "No files";
                workSheet.Cells[$"H{row}"].Value = postResponse?.CreatedAt.ToString("dd MMM yyyy");
                workSheet.Cells[$"I{row}"].Value = postResponse?.ModifiedAt.ToString("dd MMM yyyy");
                row++;
            }

            workSheet.Cells[$"A1:I{row}"].AutoFitColumns();

            await excelPackage.SaveAsync();
        }

        memoryStream.Position = 0;
        return memoryStream;

    }

    private static void EnsureCommunityIdIsValid(Guid ComId)
    {
        if (ComId == Guid.Empty)
            throw new ArgumentException("Community id cannot be empty.", nameof(ComId));
    }

    // Builds the SQL-translatable filter. Both sides are lowered so the search is case-insensitive on any database.
    private static Expression<Func<Post, bool>> BuildSearchPredicate(Guid ComId, string SearchBy, string LowerSearch)
    {
        switch (SearchBy)
        {
            case nameof(PostResponse.UserId):
                return p => p.CommunityId == ComId && p.UserId.ToString().ToLower().Contains(LowerSearch);
            case nameof(PostResponse.Title):
                return p => p.CommunityId == ComId && p.Title != null && p.Title.ToLower().Contains(LowerSearch);
            case nameof(PostResponse.Body):
                return p => p.CommunityId == ComId && p.Body != null && p.Body.ToLower().Contains(LowerSearch);
            default:
                return p => p.CommunityId == ComId;
        }
    }

    private async Task<List<PostResponse>> AddReportCounts(List<Post> Posts)
    {
        // Checking if there is posts
        if (Posts.Count == 0)
            return new List<PostResponse>();

        // Get all the ids of the posts (list sized in advance)
        List<Guid> PostIds = new List<Guid>(Posts.Count);
        foreach (Post CurrentPost in Posts)
            PostIds.Add(CurrentPost.Id);

        // Get the number of reports of every posts
        Dictionary<Guid, int> Counts = await _repository.GetReportCountsByPostIds(PostIds);

        // Set the found count and return the DTO response
        List<PostResponse> Responses = new List<PostResponse>(Posts.Count);
        foreach (Post CurrentPost in Posts)
        {
            PostResponse Response = CurrentPost.ToPostResponse();
            Response.ReportsCount = Counts.GetValueOrDefault(CurrentPost.Id);
            Responses.Add(Response);
        }

        return Responses;
    }
}
