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
- Widgets & layout: `ImGui.Button`, `ImGui.Text`, `ImGui.Label`, `ImGui.Checkbox`, `ImGui.RadioButton`, `ImGui.SliderFloat`,
  `ImGui.Separator`, `ImGui.SeparatorText`, `ImGui.NewLine`, `ImGui.Begin/End`, `ImGui.BeginChild/EndChild` (padding + clipping),
  mouse‑wheel scrolling with persisted per‑window offset
- Item queries: `ImGui.IsItemHovered/Active/Focused/Clicked/Deactivated/DeactivatedAfterEdit`
- Scrollbars: simple vertical thumb sized by visible/content, draggable
- Renderer (Silk.NET OpenGL): VAO/VBO/EBO, blending, scissor, per‑command
  texture binding + default 1×1 white fallback, scissor‑aware clear fix

### Feature Parity Snapshot

| Category | Feature | Dear ImGui | ImGuiSharp | Notes |
|----------|---------|------------|------------|-------|
| Core | Context creation & switching | ✔ | ✔ | `ImGuiContext`, `ImGui.SetCurrentContext` |
| Core | Frame lifecycle (`NewFrame/EndFrame`) | ✔ | ✔ | Matches IO/timing semantics |
| Core | Draw data (`ImDrawData`) | ✔ | ✔ | Single aggregated draw list |
| Core | ID stack & hashing | ✔ | ✔ | Window-seeded IDs, `PushID/PopID` |
| Core | Item query API (`IsItemHovered/Active/...`) | ✔ | ✔ | Includes `IsItemDeactivatedAfterEdit` |
| Input | Mouse position/buttons/wheel | ✔ | ✔ | Via `ImGui.SetMousePosition/SetMouseButtonState/AddMouseWheel` |
| Input | Keyboard state (keys, modifier detection) | ✔ | ✔ | `ImGui.SetKeyState`, modifier queries |
| Text & Fonts | Font atlas baking | ✔ | ✔ | Embedded Proggy Clean via stb_truetype |
| Text & Fonts | Text rendering (`Text`, `Label`, `TextWrapped`) | ✔ | ✔ | Wrapping, kerning, width cache |
| Widgets | Button | ✔ | ✔ | Style colours, keyboard activation |
| Widgets | Checkbox | ✔ | ✔ | Style colours, boolean toggle |
| Widgets | RadioButton | ✔ | ✔ | Circular frame + check mark |
| Widgets | SliderFloat | ✔ | ✔ | Mouse drag + keyboard modifiers/step |
| Widgets | Separator / SeparatorText | ✔ | ✔ | Style colours, centered text |
| Widgets | Text inputs (`InputText`) | ✔ | ✖ | Planned |
| Widgets | Drag widgets (`DragFloat`, etc.) | ✔ | ✖ | Planned |
| Layout | `SameLine`, `Spacing`, `NewLine` | ✔ | ✔ | Style spacing aware |
| Layout | Child windows (`BeginChild/EndChild`) | ✔ | ✔ | Scrollable regions + clamping |
| Layout | Window stack (`Begin/End`) | ✔ | ✔ | Basic window container (no title bars yet) |
| Layout | Columns/Table API | ✔ | ✖ | Not implemented |
| Navigation | Keyboard/gamepad navigation & focus | ✔ | ✖ | Focus tracking only; nav TBD |
| Navigation | Item activation via keyboard | ✔ | Partial | Buttons/slider support; full nav pending |
| Style | Colour palette | ✔ | ✔ | `ImGuiStyle.SetColor` covering core slots |
| Style | Style variables (padding, spacing, rounding) | ✔ | Partial | Item spacing/padding/text align only |
| Style | Fonts configurable | ✔ | Partial | Single embedded font; external fonts TBD |
| Advanced | Docking | ✔ | ✖ | Future milestone |
| Advanced | Tables/headers/menus | ✔ | ✖ | Future milestone |
| Advanced | Multi-viewport | ✔ | ✖ | Not planned yet |
| Backend | Rendering abstraction | ✔ | ✔ | Silk.NET OpenGL implementation |
| Backend | Multi-backend support | ✔ | Partial | Only OpenGL provider today |


See TODO.md and MILESTONE.md for roadmap and status.
For architecture details and contributor guidance (including how to add a new widget), see ARCHITECTURE.md.

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

## VS Code

- Debugging configs are included under `.vscode/`.
  - Use ".NET Launch Sample (ImGuiSharp.Sample)" to run the sample app with a debugger.
  - Use ".NET Run Tests (console)" to run the test suite from the terminal.
- Tasks: `restore`, `build`, and `test` tasks are available and wired to the launch config.

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
