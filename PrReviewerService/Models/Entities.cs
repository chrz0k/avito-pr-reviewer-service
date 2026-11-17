using System.Text.Json.Serialization;

namespace PrReviewerService.Models;

public class TeamMember
{
    private string _userId = string.Empty;
    private string _username = string.Empty;
    private bool _isActive = true;

    [JsonPropertyName("user_id")]
    public string UserId
    {
        get { return _userId; }
        set { _userId = value; }
    }

    [JsonPropertyName("username")]
    public string Username
    {
        get { return _username; }
        set { _username = value; }
    }

    [JsonPropertyName("is_active")]
    public bool IsActive
    {
        get { return _isActive; }
        set { _isActive = value; }
    }
}

public class Team
{
    private string _teamName = string.Empty;
    private List<TeamMember> _members = new List<TeamMember>();

    [JsonPropertyName("team_name")]
    public string TeamName
    {
        get { return _teamName; }
        set { _teamName = value; }
    }

    [JsonPropertyName("members")]
    public List<TeamMember> Members
    {
        get { return _members; }
        set { _members = value; }
    }
}

public class User
{
    private string _userId = string.Empty;
    private string _username = string.Empty;
    private string _teamName = string.Empty;
    private bool _isActive = true;

    [JsonPropertyName("user_id")]
    public string UserId
    {
        get { return _userId; }
        set { _userId = value; }
    }

    [JsonPropertyName("username")]
    public string Username
    {
        get { return _username; }
        set { _username = value; }
    }

    [JsonPropertyName("team_name")]
    public string TeamName
    {
        get { return _teamName; }
        set { _teamName = value; }
    }

    [JsonPropertyName("is_active")]
    public bool IsActive
    {
        get { return _isActive; }
        set { _isActive = value; }
    }
}

public class PullRequest
{
    private string _pullRequestId = string.Empty;
    private string _pullRequestName = string.Empty;
    private string _authorId = string.Empty;
    private string _status = "OPEN";
    private List<string> _assignedReviewers = new List<string>();
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime? _mergedAt;

    [JsonPropertyName("pull_request_id")]
    public string PullRequestId
    {
        get { return _pullRequestId; }
        set { _pullRequestId = value; }
    }

    [JsonPropertyName("pull_request_name")]
    public string PullRequestName
    {
        get { return _pullRequestName; }
        set { _pullRequestName = value; }
    }

    [JsonPropertyName("author_id")]
    public string AuthorId
    {
        get { return _authorId; }
        set { _authorId = value; }
    }

    [JsonPropertyName("status")]
    public string Status
    {
        get { return _status; }
        set { _status = value; }
    }

    [JsonPropertyName("assigned_reviewers")]
    public List<string> AssignedReviewers
    {
        get { return _assignedReviewers; }
        set { _assignedReviewers = value; }
    }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt
    {
        get { return _createdAt; }
        set { _createdAt = value; }
    }

    [JsonPropertyName("mergedAt")]
    public DateTime? MergedAt
    {
        get { return _mergedAt; }
        set { _mergedAt = value; }
    }
}

public class SetActiveRequest
{
    private string _userId = string.Empty;
    private bool _isActive;

    [JsonPropertyName("user_id")]
    public string UserId
    {
        get { return _userId; }
        set { _userId = value; }
    }

    [JsonPropertyName("is_active")]
    public bool IsActive
    {
        get { return _isActive; }
        set { _isActive = value; }
    }
}

public class CreatePrRequest
{
    private string _pullRequestId = string.Empty;
    private string _pullRequestName = string.Empty;
    private string _authorId = string.Empty;

    [JsonPropertyName("pull_request_id")]
    public string PullRequestId
    {
        get { return _pullRequestId; }
        set { _pullRequestId = value; }
    }

    [JsonPropertyName("pull_request_name")]
    public string PullRequestName
    {
        get { return _pullRequestName; }
        set { _pullRequestName = value; }
    }

    [JsonPropertyName("author_id")]
    public string AuthorId
    {
        get { return _authorId; }
        set { _authorId = value; }
    }
}

public class MergePrRequest
{
    private string _pullRequestId = string.Empty;

    [JsonPropertyName("pull_request_id")]
    public string PullRequestId
    {
        get { return _pullRequestId; }
        set { _pullRequestId = value; }
    }
}

public class ReassignRequest
{
    private string _pullRequestId = string.Empty;
    private string _oldUserId = string.Empty;

    [JsonPropertyName("pull_request_id")]
    public string PullRequestId
    {
        get { return _pullRequestId; }
        set { _pullRequestId = value; }
    }

    [JsonPropertyName("old_user_id")]
    public string OldUserId
    {
        get { return _oldUserId; }
        set { _oldUserId = value; }
    }
}
