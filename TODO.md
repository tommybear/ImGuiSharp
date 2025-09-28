# TODO

## Project Setup
- [x] Create `ImGuiSharp.sln` with core library, renderer backend, and sample app projects.
- [ ] Configure `Directory.Build.props` with nullable, analyzers, and deterministic builds.
- [ ] Add CI workflow for `dotnet build` and `dotnet test`.

## Core Library (ImGuiSharp)
- [x] Seed `ImGuiContext` lifecycle, input queue, and static `ImGui` facade with tests.
- [x] Extend `ImGuiContext` with time accumulation and input state (mouse, key) snapshots.
- [ ] Define public API surface mirroring Dear ImGui entry points (`ImGui`, `ImDrawList`, etc.).
- [ ] Implement context and IO management (`ImGuiContext`, input queues, frame timing).
- [ ] Port essential math primitives (`Vec2`, `Vec4`, `Color`, `Rect`, matrix helpers).
- [ ] Build ID stack, item registration, and focus/hover rules.
- [ ] Implement layout engine basics (window stacks, columns, clipping, scrolling).
- [ ] Implement baseline widgets (text, button, checkbox, slider, combo, separator).
- [ ] Add style configuration and per-widget styling overrides.
- [ ] Integrate font atlas generator and glyph rasterization pipeline.
- [ ] Expose draw command buffers and state snapshots.

## Rendering Backend
- [x] Define `IRendererBackend` abstraction (buffers, textures, shader hooks).
- [ ] Implement simple OpenGL backend using Silk.NET or OpenTK.
- [ ] Add resource lifetime management (textures, buffers, shaders) and batching.
- [ ] Provide fallback software rasterizer for headless tests (optional).

## Sample Application
- [ ] Create demo app showcasing windows, layout containers, and interactive widgets.
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
