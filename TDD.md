# Test-Driven Development Strategy

## Objectives
- Guarantee functional parity with the targeted Dear ImGui feature subset.
- Keep the clean-room implementation cleanly decoupled from rendering and platform dependencies.
- Provide fast regression feedback as the widget library grows.

## Guiding Principles
- Write the minimal failing test that captures a behaviour before implementing it.
- Prefer deterministic, pure logic tests for layout, style, and state handling.
- Mock or stub rendering and platform layers; verify draw-command outputs rather than pixels.
- Keep tests small and composable; favour table-driven tests for widget states and style variants.

## Tooling
- Use `xUnit` for unit and integration tests.
- Use `FluentAssertions` (or similar) to keep expectations expressive.
- Introduce a lightweight golden-snapshot helper for draw command buffers.
- Run all tests through `dotnet test` with coverage reporting via `coverlet`.

## Test Layers
- **Foundation tests**: geometry helpers, colour math, input lifting, font atlas builders (StbTrueTypeSharp path).
- **Widget tests**: button activation, sliders, text inputs—cover state transitions and ID resolution.
- **Layout tests**: verify item spacing, window positioning, scrolling, and clipping behaviour.
- **Rendering contract tests**: ensure the renderer interface receives the expected draw lists; verify per-command texture IDs and default texture behaviour.
- **End-to-end smoke**: small scripted UI flows executed against a headless backend to guard frame lifecycle.

## Workflow
1. Add or refine a test describing the desired behaviour.
2. Run the failing test to confirm coverage.
3. Implement the minimal change in the library or backend.
4. Re-run tests locally; iterate until green.
5. Refactor while keeping coverage green; update docs when observable behaviour changes.

## Continuous Integration Hooks
- Gate PRs on the test suite and coverage thresholds (>80% for core projects).
- Publish HTML coverage reports as artefacts for easy inspection.
- Schedule nightly runs with a higher fidelity rendering smoke test (captures vs. reference buffers).

## Test Data & Fixtures
- Maintain canonical JSON fixtures for representative frame inputs and expected draw command sequences.
- Reuse deterministic pseudo-random seeds when fuzzing widget interactions.
- Store small font atlases and glyph ranges in `tests/fixtures/` to keep repository size manageable; include an embedded Proggy Clean slice for deterministic text tests.

## Handling Regressions
- When a bug is reported, first reproduce with a failing test.
- Codify the scenario in the suite before applying the fix.
- Add a changelog entry referencing the regression test to prevent recurrence.

## Recent Additions To Cover
- Text/Label layout using ascent/line-height; kerning (once added).
- Child window scroll persistence and scrollbar dragging interactions.
- Solid-fill commands do not sample atlas (TextureId == 0 binds white texture).
