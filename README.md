# ImGuiSharp — Clean‑Room C# Immediate‑Mode GUI

ImGuiSharp is a clean‑room, idiomatic C# re‑implementation of the Dear ImGui
immediate‑mode GUI. It follows the original architecture (frame lifecycle,
ID stack, draw lists, font atlas, renderer separation), but the code here is
written from scratch in C# — not a binding or fork.

This repository also includes a copy of upstream Dear ImGui under `reference/`
purely for comparison while we build feature parity.

## Highlights (current state)

- Core runtime: `ImGui` static facade, frame lifecycle, IO (mouse/keyboard/wheel)
- Draw data: immutable draw lists + commands with per‑command `TextureId`
- Fonts & text: embedded Proggy Clean TTF (Tristan Grimmer), baked at runtime
  with StbTrueTypeSharp into an RGBA atlas; `ImGui.Text`, `ImGui.Label`
- Widgets & layout: `ImGui.Button`, `ImGui.Text`, `ImGui.Label`,
  `ImGui.Begin/End`, `ImGui.BeginChild/EndChild` (padding + clipping),
  mouse‑wheel scrolling with persisted per‑window offset
- Scrollbars: simple vertical thumb sized by visible/content, draggable
- Renderer (Silk.NET OpenGL): VAO/VBO/EBO, blending, scissor, per‑command
  texture binding + default 1×1 white fallback, scissor‑aware clear fix

See TODO.md and MILESTONE.md for roadmap and status.

## Getting started

Prerequisites: .NET 6 SDK+ and OpenGL 3.3+ drivers.

Build:

```
dotnet build ImGuiSharp.sln
```

Run the sample (opens a window with labeled buttons and a scrollable child):

```
dotnet run --project samples/ImGuiSharp.Sample
```

## Minimal usage (sketch)

```csharp
var ctx = new ImGuiContext();
ImGui.SetCurrentContext(ctx);

// per-frame
ctx.UpdateDeltaTime(deltaSeconds);
ImGui.SetDisplaySize(new Vec2(width, height));
ctx.NewFrame();

ImGui.BeginChild("panel", new Vec2(300, 180));
ImGui.Text("Hello, ImGuiSharp!");
if (ImGui.Button("Click Me", new Vec2(120, 36))) { /* ... */ }
ImGui.EndChild();

ctx.EndFrame();

var dd = ImGui.GetDrawData();
renderer.BeginFrame();
renderer.Render(dd);
renderer.EndFrame();
```

## Scope & limitations (for now)

- Early subset of Dear ImGui: buttons, text/labels, child regions, scrolling,
  simple scrollbars. Docking/tables/menus are not implemented yet.
- Text input widget not implemented yet; kerning is minimal (planned).
- API mirrors Dear ImGui where it helps familiarity but uses C# idioms where
  it improves clarity.

## Repository layout

- `src/ImGuiSharp` — core library (runtime, IO, math, fonts, drawing, widgets)
- `src/ImGuiSharp.Rendering.SilkNet` — OpenGL backend using Silk.NET
- `samples/ImGuiSharp.Sample` — window/input glue and live demo
- `reference/` — upstream Dear ImGui sources for reference only
- `docs/adr/` — architecture decisions (e.g., Silk.NET backend)

## Third‑party

- Proggy Clean TTF © Tristan Grimmer — embedded default font
  - See `third_party/fonts/ProggyClean-LICENSE.txt` and `THIRD_PARTY_NOTICES.md`.
- StbTrueTypeSharp — font baking
- Silk.NET — OpenGL + window/input bindings

## License & attribution

ImGuiSharp is a clean‑room re‑implementation inspired by Dear ImGui’s design.
Upstream sources under `reference/` are provided for comparison only.

Please see repository license and third‑party notices for included assets.

