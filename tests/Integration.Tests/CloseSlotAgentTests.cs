using System.Text.Json;
using Ngentic;
using Ngentic.NUnit;
using NUnit.Framework;
using RhMcp.Integration.Tests.Harness;

namespace RhMcp.Integration.Tests;

// Drives Claude via the local CLI against an isolated rhino-mcp-router. No
// Rhino install required — close_slot on a bogus slot only touches the router.
// Marked [Explicit] because real LLM calls cost subscription quota and are
// non-deterministic; opt in with:
//   dotnet test --filter "Category=AgentDriven"
[TestFixture]
[Explicit("Drives a real Claude session via the CLI; uses your Claude Code subscription quota.")]
[Category("AgentDriven")]
[McpDependency("rhino")]
public sealed class CloseSlotAgentTests : AgenticTestBase
{
    private string _isolatedTempDir = null!;

    protected override void ConfigureHarness()
    {
        _isolatedTempDir = RhinoRouterPaths.CreateIsolatedTempDir();
        UseMcp(
            name: "rhino",
            command: RhinoRouterPaths.ResolveBinary(),
            env: RhinoRouterPaths.IsolatedEnv(_isolatedTempDir));
        UseDefaults(maxBudgetUsd: 0.25);
    }

    [OneTimeTearDown]
    public void CleanupTempDir()
    {
        RhinoRouterPaths.TryDeleteDirectory(_isolatedTempDir);
    }

    // Regression: the agent should be able to call close_slot on a slot that
    // doesn't exist and receive a structured slot_not_found payload (not a
    // generic failure). This is the user-facing manifestation of the
    // `manager.Has` fix on this branch.
    [Test]
    public async Task agent_receives_slot_not_found_when_closing_unknown_slot()
    {
        AgentRun run = await Agent
            .WithAllowedTools("mcp__rhino__close_slot")
            .WithSystemPrompt(
                "You are an integration test harness. Call exactly the tools requested. " +
                "When reporting results, include the raw JSON the tool returned.")
            .RunAsync(
                "Close the rhino slot named 'made-up-slot-xyz'. " +
                "Do not call list_slots first — just attempt the close once and report what the tool returned.");

        Assert.That(run.Metadata?["is_error"], Is.EqualTo(false),
            $"Claude reported an error. Payload: {run.Metadata?["error_payload"]}\n" +
            $"Stderr:\n{run.Metadata?["stderr"]}");

        Assert.That(run, Did.CallTool("mcp__rhino__close_slot"));

        ToolCall close = run.ToolCalls.First(c => c.Name == "mcp__rhino__close_slot");
        Assert.That(close.Result, Does.Contain("slot_not_found"),
            $"close_slot returned without the slot_not_found marker.\nResult: {close.Result}");

        // Belt-and-braces: parse the JSON the tool returned and verify the
        // structured shape, not just the substring.
        JsonElement payload = ExtractJsonPayload(close.Result);
        Assert.That(payload.GetProperty("closed").GetBoolean(), Is.False);
        Assert.That(payload.GetProperty("error").GetString(), Is.EqualTo("slot_not_found"));
    }

    // MCP tool results may arrive wrapped in `{"content":[{"type":"text","text":"…"}]}`
    // when the result block is structured, or as a raw string when the harness
    // already flattened it. Accept either; tests care about the inner payload.
    private static JsonElement ExtractJsonPayload(string raw)
    {
        JsonDocument doc = JsonDocument.Parse(raw);
        if (doc.RootElement.ValueKind == JsonValueKind.Object
            && doc.RootElement.TryGetProperty("content", out JsonElement content)
            && content.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement block in content.EnumerateArray())
            {
                if (block.TryGetProperty("text", out JsonElement t) && t.ValueKind == JsonValueKind.String)
                {
                    return JsonDocument.Parse(t.GetString()!).RootElement;
                }
            }
        }
        return doc.RootElement;
    }
}
