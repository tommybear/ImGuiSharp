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
- Which existing managed bindings (Silk.NET, OpenTK) best balance ease of integration and maintenance?
- Should the font atlas rely on stb ports or a managed alternative like SharpFont?
- Do we target .NET 6 LTS or move to a newer runtime for span APIs and performance?

## Next Actions
- Finalise solution layout and select graphics bindings.
- Draft architectural decision records for renderer abstraction and font pipeline.
- Begin TDD cycle with context/IO management tests.

## Progress Log
- 2024-05-17: Scaffolded ImGuiSharp solution with core library, Silk.NET renderer stub, sample app, and xUnit test project.
- 2024-05-17: Replaced template stubs with `ImGuiContext`/`ImGuiIO`, defined rendering abstractions, added Silk.NET package reference, and seeded frame lifecycle tests.
