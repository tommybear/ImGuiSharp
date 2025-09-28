# ADR 0001: Adopt Silk.NET for the Reference Rendering Backend

- Status: Proposed
- Date: 2024-05-17

## Context
- The project targets a clean-room Dear ImGui implementation in C# with a cross-platform rendering backend.
- We need first-class support across Windows, macOS, and Linux without maintaining native glue per platform.
- Future integration with MonoGame should remain viable by sharing renderer abstractions and GPU resource handling patterns.
- We plan to support modern .NET runtimes (minimum .NET 6) to benefit from latest language/runtime features and long-term support.

## Decision
- Use Silk.NET as the low-level graphics/window/input abstraction for the reference backend project.
- Target OpenGL 3.3 Core Profile via Silk.NET’s OpenGL bindings to ensure wide GPU compatibility.
- Structure the renderer around a modular `IRenderPipeline` interface within the core library, with Silk.NET providing the initial concrete implementation.

## Rationale
- Silk.NET offers managed bindings for OpenGL, Vulkan, Direct3D, windowing, and input with active maintenance and broad OS coverage.
- OpenGL 3.3 Core is widely available on desktop platforms and aligns with Dear ImGui’s existing reference backend capabilities.
- Using Silk.NET reduces native interop complexity compared to hand-rolled P/Invoke layers and keeps the project fully managed.
- The abstraction allows future adapters (e.g., MonoGame, Direct3D) to share command translation logic while swapping only the backend-specific plumbing.

## Consequences
- We add a dependency on Silk.NET packages (`Silk.NET.OpenGL`, `Silk.NET.Windowing`, `Silk.NET.Input` as needed) and must watch their release cadence.
- The reference backend will require GL context management patterns compatible with Silk.NET’s windowing stack.
- MonoGame integration will likely bypass Silk.NET windowing but can reuse the same draw-command translation, keeping the core API stable.
- We need CI coverage on Windows/macOS/Linux to detect platform-specific issues in the Silk.NET layer early.

## Alternatives Considered
- SDL2 + OpenGL via custom P/Invoke (more maintenance, less idiomatic for C#).
- OpenTK (mature but slower release cadence and narrower API surface versus Silk.NET).
- Direct3D-first approach (limits macOS/Linux support for the initial release).

## Follow-Up Actions
- Scaffold `ImGuiSharp.Rendering.SilkNet` project targeting .NET 6+ and reference the required Silk.NET packages.
- Define the renderer abstraction in the core library to decouple backend-specific code.
- Document the GL state expectations and resource management patterns shared across backends.
