# Milestones

## Milestone 0 – Project Foundations (Weeks 1-2)
- Finalise feature scope for the initial release and document architecture vision.
- Scaffold solution, projects, and CI pipeline.
- Establish coding standards, testing conventions, and repository governance.

## Milestone 1 – Core Runtime (Weeks 3-6)
- Deliver context/IO management, math primitives, and ID resolution.
- Implement frame lifecycle (`NewFrame`, `EndFrame`, `Render`) with draw command buffers.
- Cover logic with unit tests and run headless smoke tests.

## Milestone 2 – Essential Widgets & Layout (Weeks 6-10)
- Implement primary widgets (text, buttons, basic inputs) and window/layout system.
- Finish clipping, scrolling, child windows, and style overrides.
- Add golden tests for layout scenarios and widget interactions.

## Milestone 3 – Rendering Backend (Weeks 10-13)
- Finalise renderer abstraction and complete the reference OpenGL backend.
- Integrate font atlas pipeline and texture uploads.
- Provide sample app rendering the minimal demo suite.

## Milestone 4 – Quality & Packaging (Weeks 13-16)
- Harden API, add documentation, and polish developer ergonomics.
- Achieve >80% coverage in core library; add performance benchmarks.
- Package first preview release (NuGet) with release notes and migration guide.

## Milestone 5 – Post-Preview Enhancements (Weeks 16+)
- Evaluate advanced features (tables, docking) and prioritise follow-up work.
- Explore additional rendering backends (Direct3D, Vulkan) and host integrations.
- Gather community feedback and plan subsequent iterations.
