using System;
using System.Collections.Generic;
using DebugProbe.AspNetCore.Internal.Rendering;
using DebugProbe.AspNetCore.Models;
using DebugProbe.AspNetCore.Options;
using DebugProbe.AspNetCore.Storage;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace DebugProbe.AspNetCore.Tests.Rendering;

public class HtmlRendererEnvDiffTests
{
    [Fact]
    public void Render_index_page_with_AutoEnvironmentDiff_disabled_does_not_compare()
    {
        // Arrange
        var options = new DebugProbeOptions { AutoEnvironmentDiff = false };
        var store = new DebugEntryStore(options);

        var entry1 = new DebugEntry
        {
            Id = "1",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5),
            Method = "GET",
            Path = "/api/users",
            ResponseBody = "{\"status\": \"ok\"}"
        };
        var entry2 = new DebugEntry
        {
            Id = "2",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-2),
            Method = "GET",
            Path = "/api/users",
            ResponseBody = "{\"status\": \"error\"}"
        };

        var devEnv = new DebugEnvironment { Environment = "Development" };
        var prodEnv = new DebugEnvironment { Environment = "Production" };

        store.Add(entry1, devEnv);
        store.Add(entry2, prodEnv);

        // Act
        var html = HtmlRenderer.RenderIndexPage(store.GetAll(), options);

        // Assert
        Assert.DoesNotContain("class=\"dbp-badge dbp-badge-envdiff\"", html);
    }

    [Fact]
    public void Render_index_page_with_same_environment_shows_no_badge()
    {
        // Arrange
        var options = new DebugProbeOptions { AutoEnvironmentDiff = true };
        var store = new DebugEntryStore(options);

        var entry1 = new DebugEntry
        {
            Id = "1",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5),
            Method = "GET",
            Path = "/api/users",
            ResponseBody = "{\"status\": \"ok\"}"
        };
        var entry2 = new DebugEntry
        {
            Id = "2",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-2),
            Method = "GET",
            Path = "/api/users",
            ResponseBody = "{\"status\": \"error\"}"
        };

        var devEnv = new DebugEnvironment { Environment = "Development" };

        store.Add(entry1, devEnv);
        store.Add(entry2, devEnv);

        // Act
        var html = HtmlRenderer.RenderIndexPage(store.GetAll(), options);

        // Assert
        Assert.DoesNotContain("class=\"dbp-badge dbp-badge-envdiff\"", html);
    }

    [Fact]
    public void Render_index_page_with_different_environments_and_identical_payloads_shows_no_badge()
    {
        // Arrange
        var options = new DebugProbeOptions { AutoEnvironmentDiff = true };
        var store = new DebugEntryStore(options);

        var entry1 = new DebugEntry
        {
            Id = "1",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5),
            Method = "GET",
            Path = "/api/users",
            ResponseBody = "{\"status\": \"ok\"}"
        };
        var entry2 = new DebugEntry
        {
            Id = "2",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-2),
            Method = "GET",
            Path = "/api/users",
            ResponseBody = "{\"status\": \"ok\"}"
        };

        var devEnv = new DebugEnvironment { Environment = "Development" };
        var prodEnv = new DebugEnvironment { Environment = "Production" };

        store.Add(entry1, devEnv);
        store.Add(entry2, prodEnv);

        // Act
        var html = HtmlRenderer.RenderIndexPage(store.GetAll(), options);

        // Assert
        Assert.DoesNotContain("class=\"dbp-badge dbp-badge-envdiff\"", html);
    }

    [Fact]
    public void Render_index_page_with_different_environments_and_differing_payloads_shows_badge()
    {
        // Arrange
        var options = new DebugProbeOptions { AutoEnvironmentDiff = true };
        var store = new DebugEntryStore(options);

        var entry1 = new DebugEntry
        {
            Id = "1",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5),
            Method = "GET",
            Path = "/api/users",
            ResponseBody = "{\"status\": \"ok\"}"
        };
        var entry2 = new DebugEntry
        {
            Id = "2",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-2),
            Method = "GET",
            Path = "/api/users",
            ResponseBody = "{\"status\": \"error\"}"
        };

        var devEnv = new DebugEnvironment { Environment = "Development" };
        var prodEnv = new DebugEnvironment { Environment = "Production" };

        store.Add(entry1, devEnv);
        store.Add(entry2, prodEnv);

        // Act
        var html = HtmlRenderer.RenderIndexPage(store.GetAll(), options);

        // Assert
        Assert.Contains("class=\"dbp-badge dbp-badge-envdiff\"", html);
        Assert.Contains("Payload differences detected between: Production, Development", html);
    }

    [Fact]
    public void Route_normalization_matches_slash_and_query_string()
    {
        // Arrange
        var options = new DebugProbeOptions { AutoEnvironmentDiff = true };
        var store = new DebugEntryStore(options);

        // entry1 path has trailing slash, entry2 path has query string. They should normalize to /api/users
        var entry1 = new DebugEntry
        {
            Id = "1",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5),
            Method = "GET",
            Path = "/api/users/",
            ResponseBody = "{\"status\": \"ok\"}"
        };
        var entry2 = new DebugEntry
        {
            Id = "2",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-2),
            Path = "/api/users?id=123",
            Method = "GET",
            ResponseBody = "{\"status\": \"error\"}"
        };

        var devEnv = new DebugEnvironment { Environment = "Development" };
        var prodEnv = new DebugEnvironment { Environment = "Production" };

        store.Add(entry1, devEnv);
        store.Add(entry2, prodEnv);

        // Act
        var html = HtmlRenderer.RenderIndexPage(store.GetAll(), options);

        // Assert
        Assert.Contains("class=\"dbp-badge dbp-badge-envdiff\"", html);
    }

    [Fact]
    public void Empty_store_and_single_entry_show_no_badge()
    {
        // Arrange
        var options = new DebugProbeOptions { AutoEnvironmentDiff = true };
        var store = new DebugEntryStore(options);

        // Act & Assert (empty)
        var htmlEmpty = HtmlRenderer.RenderIndexPage(store.GetAll(), options);
        Assert.DoesNotContain("class=\"dbp-badge dbp-badge-envdiff\"", htmlEmpty);

        // Add single entry
        var entry = new DebugEntry
        {
            Id = "1",
            Timestamp = DateTimeOffset.UtcNow,
            Method = "GET",
            Path = "/api/users",
            ResponseBody = "{\"status\": \"ok\"}"
        };
        store.Add(entry, new DebugEnvironment { Environment = "Development" });

        // Act & Assert (single entry)
        var htmlSingle = HtmlRenderer.RenderIndexPage(store.GetAll(), options);
        Assert.DoesNotContain("class=\"dbp-badge dbp-badge-envdiff\"", htmlSingle);
    }
}
