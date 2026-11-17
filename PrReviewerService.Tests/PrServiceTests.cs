using PrReviewerService.Models;
using PrReviewerService.Services;
using Xunit;

namespace PrReviewerService.Tests;

public class PrServiceTests
{
    [Fact]
    public void CreatePr_AssignsMaxTwoActiveReviewers_WithoutAuthor()
    {
        var service = new PrService();
        service.AddTeam(new Team
        {
            TeamName = "backend",
            Members = new List<TeamMember>
            {
                new TeamMember { UserId = "u1", Username = "Egor", IsActive = true },
                new TeamMember { UserId = "u2", Username = "Ivan", IsActive = true },
                new TeamMember { UserId = "u3", Username = "Anna", IsActive = true },
                new TeamMember { UserId = "u4", Username = "Oleg", IsActive = false }
            }
        });

        var res = service.CreatePullRequest(new CreatePrRequest
        {
            PullRequestId = "pr-1",
            PullRequestName = "Fix bug",
            AuthorId = "u1"
        });

        Assert.True(res.IsSuccess);
        Assert.Equal(2, res.Data!.AssignedReviewers.Count);
        Assert.DoesNotContain("u1", res.Data.AssignedReviewers);
        Assert.DoesNotContain("u4", res.Data.AssignedReviewers);
    }

    [Fact]
    public void CreatePr_WhenAuthorAlone_ReturnsEmptyList()
    {
        var service = new PrService();
        service.AddTeam(new Team
        {
            TeamName = "solo",
            Members = new List<TeamMember>
            {
                new TeamMember { UserId = "u1", Username = "Egor", IsActive = true }
            }
        });

        var res = service.CreatePullRequest(new CreatePrRequest
        {
            PullRequestId = "pr-2",
            PullRequestName = "Init",
            AuthorId = "u1"
        });

        Assert.True(res.IsSuccess);
        Assert.Empty(res.Data!.AssignedReviewers);
    }

    [Fact]
    public void ReassignPr_AfterMerge_Returns409Error()
    {
        var service = new PrService();
        service.AddTeam(new Team
        {
            TeamName = "core",
            Members = new List<TeamMember>
            {
                new TeamMember { UserId = "u1", Username = "A", IsActive = true },
                new TeamMember { UserId = "u2", Username = "B", IsActive = true },
                new TeamMember { UserId = "u3", Username = "C", IsActive = true }
            }
        });

        var created = service.CreatePullRequest(new CreatePrRequest
        {
            PullRequestId = "pr-3",
            PullRequestName = "Feature",
            AuthorId = "u1"
        });

        service.MergePullRequest(new MergePrRequest { PullRequestId = "pr-3" });

        var reassignRes = service.ReassignReviewer(new ReassignRequest
        {
            PullRequestId = "pr-3",
            OldUserId = created.Data!.AssignedReviewers[0]
        });

        Assert.False(reassignRes.IsSuccess);
        Assert.Equal(409, reassignRes.StatusCode);
        Assert.Equal("PR_MERGED", reassignRes.ErrorCode);
    }

    [Fact]
    public void AddTeam_DuplicateUserId_Returns400Error()
    {
        var service = new PrService();
        var res = service.AddTeam(new Team
        {
            TeamName = "test",
            Members = new List<TeamMember>
            {
                new TeamMember { UserId = "u1", Username = "Egor", IsActive = true },
                new TeamMember { UserId = "u1", Username = "Egor2", IsActive = true }
            }
        });

        Assert.False(res.IsSuccess);
        Assert.Equal(400, res.StatusCode);
    }
}
