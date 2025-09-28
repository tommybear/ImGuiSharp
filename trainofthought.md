# Planning Notes

## Intent
- Deliver a clean-room C# re-implementation of Dear ImGui with a minimal yet functional rendering backend.
- Preserve the immediate mode workflow while adapting APIs to idiomatic C#.

## Key Considerations
- Balance feature parity with achievable scope; prioritise core widgets and layout before advanced modules.
- Keep rendering backend pluggable so alternative graphics APIs can be supported later.
- Maintain deterministic testing harnesses to avoid dependence on pixel-perfect comparisons.

## Approach Outline
1. Establish project structure and shared tooling to keep development friction low.
2. Port foundational runtime pieces (context, IO, math) to unblock higher-level features.
3. Build widget/layout layers incrementally, verifying state machines through tests.
4. Define a narrow renderer contract and back it with a simple OpenGL implementation first.
5. Use the sample app and golden tests to anchor behaviour while iterating.

## Open Questions
- How should the input event queue scale to support multiple contexts and platform backends without excessive copying?
- Should the font atlas rely on stb ports or a managed alternative like SharpFont?
- Do we target .NET 6 LTS or move to a newer runtime for span APIs and performance?

## Next Actions
- Finalise solution layout and select graphics bindings.
- Draft architectural decision records for renderer abstraction and font pipeline.
- Begin TDD cycle with context/IO management tests.

## Progress Log
- 2024-05-17: Scaffolded ImGuiSharp solution with core library, Silk.NET renderer stub, sample app, and xUnit test project.
- 2024-05-17: Replaced template stubs with `ImGuiContext`/`ImGuiIO`, defined rendering abstractions, added Silk.NET package reference, and seeded frame lifecycle tests.
- 2024-05-17: Extended context with input queues and exposed ImGui static facade backed by unit tests.
- 2024-05-17: Added timing, mouse, and key state tracking to context and wired facade helpers to expose them.
- 2024-05-17: Defined draw data structures with vertex/index buffers, clip rects, and texture ids; added tests.
- 2024-05-17: Added Vec2/Vec4/Color primitives and integrated them into IO, context, and draw data pipelines.
- 2024-05-17: Implemented button widget with ID stack, hover/active tracking, and unit tests.
