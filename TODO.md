# TODO

## Project Setup
- [x] Create `ImGuiSharp.sln` with core library, renderer backend, and sample app projects.
- [ ] Configure `Directory.Build.props` with nullable, analyzers, and deterministic builds.
- [ ] Add CI workflow for `dotnet build` and `dotnet test`.

## Core Library (ImGuiSharp)
- [x] Seed `ImGuiContext` lifecycle, input queue, and static `ImGui` facade with tests.
- [x] Extend `ImGuiContext` with time accumulation and input state (mouse, key) snapshots.
- [x] Introduce core math primitives (Vec2, Vec4, Color) and wire into context/IO.
- [ ] Define public API surface mirroring Dear ImGui entry points (`ImGui`, `ImDrawList`, etc.).
- [ ] Implement context and IO management (`ImGuiContext`, input queues, frame timing).
- [ ] Port essential math primitives (`Vec2`, `Vec4`, `Color`, `Rect`, matrix helpers).
- [ ] Build ID stack, item registration, and focus/hover rules.
- [ ] Implement layout engine basics (window stacks, columns, clipping, scrolling).
  - [x] Add minimal window/child stack with clip rect + padding.
  - [x] Mouse-wheel scrolling with per-window persisted offset.
  - [x] Simple vertical scrollbar with dragging.
- [ ] Implement baseline widgets (text, button, checkbox, slider, combo, separator).
  - [x] Add button widget with ID stack handling and hover/press tests.
  - [x] Text + Label helpers using embedded font atlas.
  - [x] Checkbox (bool toggle with label) with unit tests.
  - [x] SliderFloat (drag to set value) with unit tests.
  - [x] Separator/NewLine/SeparatorText helpers with layout/draw tests.
- [ ] Add style configuration and per-widget styling overrides.
- [ ] Integrate font atlas generator and glyph rasterization pipeline.
  - [x] Embed Proggy Clean TTF; bake ASCII atlas via StbTrueTypeSharp.
  - [x] Register atlas as GL texture and bind per draw command.
  - [x] Fix solid fills to use default white texture (no atlas bleed).
- [x] Expose draw command buffers and state snapshots.

## Rendering Backend
- [x] Define `IRendererBackend` abstraction (buffers, textures, shader hooks).
- [ ] Implement simple OpenGL backend using Silk.NET or OpenTK.
  - [x] Allocate core GL resources (VAO/VBO/EBO, shaders) and stub buffer uploads in Silk.NET pipeline.
  - [x] Per-command texture binding and default 1x1 white texture.
  - [x] Scissor-aware clear fix (disable scissor during clear).
- [ ] Add resource lifetime management (textures, buffers, shaders) and batching.
- [ ] Provide fallback software rasterizer for headless tests (optional).

## Sample Application
- [ ] Create demo app showcasing windows, layout containers, and interactive widgets.
  - [x] Pipe Silk.NET mouse/keyboard input into ImGui context for interactivity.
  - [x] Hook sample window to Silk.NET pipeline with test triangle render.
  - [x] Demo: buttons with text labels; scrollable child list with wheel + draggable scrollbar.
- [ ] Include comparison view replicating a subset of Dear ImGui demo windows.
- [ ] Add automated UI script to exercise key interactions.

## Tooling & Quality
- [ ] Port selected stb utilities or integrate SharpFont for font handling.
- [ ] Write extensive unit tests for layout math and widget state machines.
- [ ] Add serialization harness for draw command buffers to support snapshot tests.
- [ ] Document architecture decisions (ADR log) and coding guidelines (CONTRIBUTING).
- [ ] Prepare initial NuGet packaging metadata and versioning scheme.

## Stretch Goals
- [ ] Add docking, tables, and advanced widgets after core parity is achieved.
- [ ] Provide Direct3D 11 backend for Windows-native performance.
- [ ] Investigate Unity and MonoGame hosting bridges for broader adoption.
