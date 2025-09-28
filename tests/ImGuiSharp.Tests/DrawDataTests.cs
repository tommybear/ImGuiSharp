using System;
using ImGuiSharp.Rendering;
using Xunit;

namespace ImGuiSharp.Tests;

public sealed class DrawDataTests
{
    [Fact]
    public void DrawList_StoresImmutableCopies()
    {
        var vertices = new[]
        {
            new ImGuiVertex(0f, 0f, 0f, 0f, 0xFFFFFFFFu),
            new ImGuiVertex(1f, 0f, 1f, 0f, 0xFFFFFFFFu)
        };
        var indices = new ushort[] { 0, 1 };
        var command = new ImGuiDrawCommand(2, new ImGuiRect(0f, 0f, 1f, 1f), IntPtr.Zero);

        var list = new ImGuiDrawList(vertices, indices, new[] { command });

        Assert.Equal(2, list.Vertices.Length);
        Assert.Equal(2, list.Indices.Length);
        Assert.Single(list.Commands);

        vertices[0] = default;
        indices[0] = 42;

        Assert.Equal(0f, list.Vertices.Span[0].PositionX);
        Assert.Equal(0f, list.Vertices.Span[0].PositionY);
        Assert.Equal(0, list.Indices.Span[0]);
    }

    [Fact]
    public void DrawData_ComputesAggregateCounts()
    {
        var list = new ImGuiDrawList(
            new[] { new ImGuiVertex(0f, 0f, 0f, 0f, 0xFFFFFFFFu) },
            new ushort[] { 0 },
            new[] { new ImGuiDrawCommand(1, new ImGuiRect(0f, 0f, 1f, 1f), IntPtr.Zero) });

        var drawData = new ImGuiDrawData(new[] { list }, new ImGuiRect(0, 0, 1280, 720));

        Assert.Equal(1, drawData.TotalVtxCount);
        Assert.Equal(1, drawData.TotalIdxCount);
        Assert.Equal(new ImGuiRect(0, 0, 1280, 720), drawData.DisplayRect);
    }
}
