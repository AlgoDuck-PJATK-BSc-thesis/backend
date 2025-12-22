using System.Reflection;
using AlgoDuck.Modules.Cohort.Shared.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Hubs;

public sealed class CohortChatHubTests
{
    [Fact]
    public void CohortChatHub_InheritsFromHub()
    {
        typeof(CohortChatHub)
            .BaseType
            .Should()
            .Be(typeof(Hub));
    }

    [Fact]
    public void CohortChatHub_HasAuthorizeAttribute()
    {
        var attributes = typeof(CohortChatHub)
            .GetCustomAttributes(inherit: true)
            .OfType<AuthorizeAttribute>()
            .ToArray();

        attributes.Should().NotBeEmpty();
    }

    [Fact]
    public void CohortChatHub_DeclaresPublicHubMethods()
    {
        var methods = typeof(CohortChatHub)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .ToArray();

        methods.Should().NotBeEmpty();
    }
}