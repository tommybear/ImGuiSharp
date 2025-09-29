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

Legend: ✔ implemented · ✗ not yet · ◑ partial/limited

| Category | Feature | Dear ImGui | ImGuiSharp | Notes |
|----------|---------|------------|------------|-------|
| **Core Runtime** | Multiple contexts & switching | ✔ | ✔ | `ImGuiContext`, `ImGui.SetCurrentContext` |
| | Frame lifecycle (`NewFrame/EndFrame`) | ✔ | ✔ | IO, delta time, frame timers |
| | Draw data (`ImDrawData`) | ✔ | ✔ | Single aggregated draw list per frame |
| | Multi-draw list support | ✔ | ◑ | One combined draw list today |
| | ID stack & hashing | ✔ | ✔ | Window-seeded IDs, `PushID/PopID` |
| | Item query API (`IsItemHovered/Active/...`) | ✔ | ✔ | Includes `IsItemDeactivatedAfterEdit` |
| | Item snippets (`IsItemToggledOpen` etc.) | ✔ | ✗ | Pending |
| **Input** | Mouse position/buttons/wheel | ✔ | ✔ | `SetMousePosition`, `SetMouseButtonState`, `AddMouseWheel` |
| | Keyboard state & modifiers | ✔ | ✔ | `SetKeyState`, modifier detection |
| | Input queue (`AddInputEvent`) | ✔ | ✔ | Basic event forwarding |
| | Gamepad navigation | ✔ | ✗ | Not implemented |
| **Text & Fonts** | Embedded font atlas | ✔ | ✔ | Proggy Clean via stb_truetype |
| | External font loading | ✔ | ✗ | Future work |
| | Kerning & width measurement | ✔ | ✔ | ASCII kerning, width cache |
| | Text helpers (`Text`, `Label`, `TextWrapped`, `CalcTextSize`) | ✔ | ✔ | Wrap width via content region |
| **Widgets** | Button / Checkbox / Radio | ✔ | ✔ | Style-driven visuals |
| | SliderFloat | ✔ | ✔ | Mouse drag + keyboard modifiers/step |
| | Separator / SeparatorText | ✔ | ✔ | Centered labels, style colours |
| | ProgressBar | ✔ | ✗ | Planned |
| | Combo/Selectables | ✔ | ✗ | Planned |
| | InputText | ✔ | ✗ | Planned |
| | Drag widgets (`DragFloat`, etc.) | ✔ | ✗ | Planned |
| | Tree nodes / collapsing headers | ✔ | ✗ | Planned |
| | Menus / MenuBar / Popup | ✔ | ✗ | Planned |
| **Layout** | `SameLine`, `Spacing`, `NewLine` | ✔ | ✔ | Style spacing aware |
| | Child windows (`BeginChild/EndChild`) | ✔ | ✔ | Scrollable regions + persistent scroll |
| | Window containers (`Begin/End`) | ✔ | ✔ | Basic window stack (no title bars yet) |
| | Columns, tables, layout helpers | ✔ | ✗ | Future milestone |
| | Multi-viewport/platform windows | ✔ | ✗ | Not yet planned |
| **Navigation & Focus** | Hover/focus state reporting | ✔ | ✔ | Item status flags |
| | Keyboard navigation | ✔ | ✗ | Focus tracking only; nav TBD |
| | Gamepad navigation | ✔ | ✗ | Not implemented |
| **Style** | Colour palette editing | ✔ | ✔ | `ImGuiStyle.SetColor` for core slots |
| | Style variables (`FramePadding`, `ItemSpacing`, etc.) | ✔ | ◑ | Padding/spacing/text align exposed; style stacks pending |
| | Style stacks (`PushStyleVar/Color`) | ✔ | ✗ | Not implemented |
| | Theme editing helpers (`ImGui::StyleColors*`) | ✔ | ✗ | To be added |
| **Text/Item Queries** | Deactivated/AfterEdit / Active status | ✔ | ✔ | Per-item status flags |
| | Storage queries (`GetItemRect`, etc.) | ✔ | ◑ | Rect via `LastItemRect`; more helpers pending |
| **Rendering Backend** | Draw command submission | ✔ | ✔ | Silk.NET OpenGL reference backend |
| | Texture registration | ✔ | ✔ | `RegisterTexture/UnregisterTexture` |
| | Multi-backend abstraction | ✔ | ◑ | Interface defined; additional backends TBD |
| | Software renderer | ✔ | ✗ | Not implemented |
| **Advanced Features** | Docking | ✔ | ✗ | Future milestone |
| | Tables API | ✔ | ✗ | Planned |
| | Menus & popups | ✔ | ✗ | Planned |
| | ImPlot/ImNodes integrations | ✔ | ✗ | Out of scope for now |


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
