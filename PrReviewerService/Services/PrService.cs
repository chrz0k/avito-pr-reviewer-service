using PrReviewerService.Models;

namespace PrReviewerService.Services;

public class ServiceResult<T>
{
    private int _statusCode;
    private string _errorCode = string.Empty;
    private string _errorMessage = string.Empty;
    private T? _data;

    public int StatusCode { get { return _statusCode; } set { _statusCode = value; } }
    public string ErrorCode { get { return _errorCode; } set { _errorCode = value; } }
    public string ErrorMessage { get { return _errorMessage; } set { _errorMessage = value; } }
    public T? Data { get { return _data; } set { _data = value; } }

    public bool IsSuccess { get { return _statusCode >= 200 && _statusCode < 300; } }

    public static ServiceResult<T> Ok(T data, int statusCode = 200)
    {
        return new ServiceResult<T> { StatusCode = statusCode, Data = data };
    }

    public static ServiceResult<T> Fail(int statusCode, string errorCode, string message)
    {
        return new ServiceResult<T>
        {
            StatusCode = statusCode,
            ErrorCode = errorCode,
            ErrorMessage = message
        };
    }
}

public class PrService
{
    private readonly Dictionary<string, Team> _teams = new Dictionary<string, Team>();
    private readonly Dictionary<string, User> _users = new Dictionary<string, User>();
    private readonly Dictionary<string, PullRequest> _pullRequests = new Dictionary<string, PullRequest>();
    private readonly Random _random = new Random();

    public ServiceResult<Team> AddTeam(Team? team)
    {
        if (team == null || string.IsNullOrWhiteSpace(team.TeamName))
        {
            return ServiceResult<Team>.Fail(400, "INVALID_INPUT", "Название команды не должно быть пустым");
        }

        if (team.Members == null || team.Members.Count == 0)
        {
            return ServiceResult<Team>.Fail(400, "INVALID_INPUT", "В команде должен быть хотя бы один участник");
        }

        if (_teams.ContainsKey(team.TeamName))
        {
            return ServiceResult<Team>.Fail(400, "TEAM_EXISTS", "Команда с таким названием уже существует");
        }

        team.TeamName = team.TeamName.Trim();

        HashSet<string> ids = new HashSet<string>();
        for (int i = 0; i < team.Members.Count; i++)
        {
            TeamMember member = team.Members[i];
            if (string.IsNullOrWhiteSpace(member.UserId) || string.IsNullOrWhiteSpace(member.Username))
            {
                return ServiceResult<Team>.Fail(400, "INVALID_INPUT", "У всех участников должны быть user_id и username");
            }

            member.UserId = member.UserId.Trim();
            member.Username = member.Username.Trim();

            if (!ids.Add(member.UserId))
            {
                return ServiceResult<Team>.Fail(400, "INVALID_INPUT", "Повторяющийся user_id в списке команды: " + member.UserId);
            }
        }

        for (int i = 0; i < team.Members.Count; i++)
        {
            TeamMember member = team.Members[i];
            User user = new User
            {
                UserId = member.UserId,
                Username = member.Username,
                TeamName = team.TeamName,
                IsActive = member.IsActive
            };
            _users[user.UserId] = user;
        }

        _teams[team.TeamName] = team;
        return ServiceResult<Team>.Ok(team, 201);
    }

    public ServiceResult<Team> GetTeam(string? teamName)
    {
        if (string.IsNullOrWhiteSpace(teamName))
        {
            return ServiceResult<Team>.Fail(400, "INVALID_INPUT", "Не передан параметр team_name");
        }

        if (!_teams.ContainsKey(teamName))
        {
            return ServiceResult<Team>.Fail(404, "NOT_FOUND", "Команда не найдена");
        }

        return ServiceResult<Team>.Ok(_teams[teamName]);
    }

    public ServiceResult<User> SetUserActive(SetActiveRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.UserId))
        {
            return ServiceResult<User>.Fail(400, "INVALID_INPUT", "Не указан user_id");
        }

        if (!_users.ContainsKey(request.UserId))
        {
            return ServiceResult<User>.Fail(404, "NOT_FOUND", "Пользователь не найден");
        }

        User user = _users[request.UserId];
        user.IsActive = request.IsActive;

        if (_teams.ContainsKey(user.TeamName))
        {
            Team team = _teams[user.TeamName];
            for (int i = 0; i < team.Members.Count; i++)
            {
                if (team.Members[i].UserId == user.UserId)
                {
                    team.Members[i].IsActive = request.IsActive;
                    break;
                }
            }
        }

        return ServiceResult<User>.Ok(user);
    }

    public ServiceResult<PullRequest> CreatePullRequest(CreatePrRequest? request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.PullRequestId) ||
            string.IsNullOrWhiteSpace(request.PullRequestName) ||
            string.IsNullOrWhiteSpace(request.AuthorId))
        {
            return ServiceResult<PullRequest>.Fail(400, "INVALID_INPUT", "Заполните pull_request_id, pull_request_name и author_id");
        }

        if (_pullRequests.ContainsKey(request.PullRequestId))
        {
            return ServiceResult<PullRequest>.Fail(409, "PR_EXISTS", "Pull Request с таким ID уже существует");
        }

        if (!_users.ContainsKey(request.AuthorId))
        {
            return ServiceResult<PullRequest>.Fail(404, "NOT_FOUND", "Автор не найден");
        }

        User author = _users[request.AuthorId];
        if (!_teams.ContainsKey(author.TeamName))
        {
            return ServiceResult<PullRequest>.Fail(404, "NOT_FOUND", "Команда автора не найдена");
        }
        
        Team team = _teams[author.TeamName];
        List<string> candidates = new List<string>();
        for (int i = 0; i < team.Members.Count; i++)
        {
            TeamMember member = team.Members[i];
            if (member.UserId != author.UserId && member.IsActive)
            {
                candidates.Add(member.UserId);
            }
        }

        List<string> reviewers = new List<string>();
        int countToPick = candidates.Count < 2 ? candidates.Count : 2;
        for (int i = 0; i < countToPick; i++)
        {
            int index = _random.Next(candidates.Count);
            reviewers.Add(candidates[index]);
            candidates.RemoveAt(index);
        }

        PullRequest pr = new PullRequest
        {
            PullRequestId = request.PullRequestId,
            PullRequestName = request.PullRequestName,
            AuthorId = request.AuthorId,
            Status = "OPEN",
            AssignedReviewers = reviewers,
            CreatedAt = DateTime.UtcNow
        };

        _pullRequests[pr.PullRequestId] = pr;
        return ServiceResult<PullRequest>.Ok(pr, 201);
    }

    public ServiceResult<PullRequest> MergePullRequest(MergePrRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.PullRequestId))
        {
            return ServiceResult<PullRequest>.Fail(400, "INVALID_INPUT", "Не указан pull_request_id");
        }

        if (!_pullRequests.ContainsKey(request.PullRequestId))
        {
            return ServiceResult<PullRequest>.Fail(404, "NOT_FOUND", "Pull Request не найден");
        }

        PullRequest pr = _pullRequests[request.PullRequestId];
        if (pr.Status != "MERGED")
        {
            pr.Status = "MERGED";
            pr.MergedAt = DateTime.UtcNow;
        }

        return ServiceResult<PullRequest>.Ok(pr);
    }

    public ServiceResult<(PullRequest Pr, string ReplacedBy)> ReassignReviewer(ReassignRequest? request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.PullRequestId) ||
            string.IsNullOrWhiteSpace(request.OldUserId))
        {
            return ServiceResult<(PullRequest, string)>.Fail(400, "INVALID_INPUT", "Укажите pull_request_id и old_user_id");
        }

        if (!_pullRequests.ContainsKey(request.PullRequestId))
        {
            return ServiceResult<(PullRequest, string)>.Fail(404, "NOT_FOUND", "Pull Request не найден");
        }

        if (!_users.ContainsKey(request.OldUserId))
        {
            return ServiceResult<(PullRequest, string)>.Fail(404, "NOT_FOUND", "Пользователь не найден");
        }

        PullRequest pr = _pullRequests[request.PullRequestId];
        if (pr.Status == "MERGED")
        {
            return ServiceResult<(PullRequest, string)>.Fail(409, "PR_MERGED", "Нельзя менять ревьюеров после merge");
        }

        int oldIndex = pr.AssignedReviewers.IndexOf(request.OldUserId);
        if (oldIndex == -1)
        {
            return ServiceResult<(PullRequest, string)>.Fail(409, "NOT_ASSIGNED", "Этот пользователь не назначен ревьюером на данный PR");
        }

        User oldReviewer = _users[request.OldUserId];
        Team team = _teams[oldReviewer.TeamName];

        List<string> candidates = new List<string>();
        for (int i = 0; i < team.Members.Count; i++)
        {
            TeamMember member = team.Members[i];
            if (member.UserId != pr.AuthorId &&
                member.UserId != request.OldUserId &&
                !pr.AssignedReviewers.Contains(member.UserId) &&
                member.IsActive)
            {
                candidates.Add(member.UserId);
            }
        }

        if (candidates.Count == 0)
        {
            return ServiceResult<(PullRequest, string)>.Fail(409, "NO_CANDIDATE", "В команде нет свободных активных кандидатов для замены");
        }

        string newUserId = candidates[_random.Next(candidates.Count)];
        pr.AssignedReviewers[oldIndex] = newUserId;

        return ServiceResult<(PullRequest, string)>.Ok((pr, newUserId));
    }

    public ServiceResult<List<PullRequest>> GetUserReviews(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ServiceResult<List<PullRequest>>.Fail(400, "INVALID_INPUT", "Не передан параметр user_id");
        }

        if (!_users.ContainsKey(userId))
        {
            return ServiceResult<List<PullRequest>>.Fail(404, "NOT_FOUND", "Пользователь не найден");
        }

        List<PullRequest> result = new List<PullRequest>();
        foreach (PullRequest pr in _pullRequests.Values)
        {
            if (pr.AssignedReviewers.Contains(userId))
            {
                result.Add(pr);
            }
        }

        return ServiceResult<List<PullRequest>>.Ok(result);
    }
}
