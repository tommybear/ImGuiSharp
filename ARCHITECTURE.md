# Architecture Overview

This document explains how the clean‑room C# implementation maps the core
concepts of Dear ImGui to ImGuiSharp components and how the renderer backend
(Silk.NET OpenGL) consumes draw data.

## Core layers

- API surface (`ImGui`)
  - Static entry points similar to Dear ImGui (e.g., `Begin/End`, `Button`,
    `Text`, `BeginChild/EndChild`, clip helpers).
  - Methods forward to the current `ImGuiContext`.

- Context (`ImGuiContext`)
  - Owns frame lifecycle (`NewFrame`/`EndFrame`), `IO` (delta time, display size,
    mouse/keys/wheel), and global state (ID stack, hover/active IDs).
  - Maintains a simple window/child stack with padding, clip rect, and a
    persisted `ScrollY` per window name.
  - Converts immediate‑mode calls into draw‑list primitives via a builder and
    exposes immutable `ImGuiDrawData` at the end of a frame.

- Draw data (`Rendering/*`)
  - `ImGuiDrawList` carries interleaved vertex/index buffers and an array of
    `ImGuiDrawCommand { ElementCount, ClipRect, TextureId }`.
  - Commands are emitted in submission order; a command’s `TextureId == 0`
    indicates the default 1×1 white texture (for solid fills).

- Draw‑list builder
  - Accumulates vertices/indices and groups triangles into commands while
    tracking a current clip rect and a current texture.
  - Offers helpers for solid rects and textured quads used by text rendering.

## Text pipeline

- Embedded font
  - Proggy Clean TTF is embedded as a resource and loaded via
    `Fonts/EmbeddedProggyClean`.

- Atlas baking
  - `Fonts/FontAtlasBuilder` uses StbTrueTypeSharp to bake ASCII (32..126) into
    an 8‑bit alpha atlas, then expands it to RGBA (white RGB + alpha coverage).
  - The atlas stores glyph advances, offsets (relative to baseline) and UVs.

- Rendering text
  - `ImGuiContext.AddText` sets the builder’s current texture to the font atlas
    and emits one quad per glyph with proper UVs and baseline positioning.
  - `ImGui.Text` renders at the current cursor and advances by line height;
    `ImGui.Label` renders absolute text without affecting the cursor.

## Layout & scrolling

- `Begin/End`, `BeginChild/EndChild`
  - On `Begin*`, the context pushes a window state, restores persisted `ScrollY`,
    applies wheel deltas if hovered, moves the cursor to content origin
    (pos + padding − scrollY), and pushes a clip rect for the window.
  - On `End*`, the context computes content height from the cursor delta,
    clamps `ScrollY` to the visible range, optionally draws a simple vertical
    scrollbar (track + draggable thumb), persists `ScrollY`, restores cursor,
    and pops the clip rect.

- Scrollbar
  - The thumb height is proportional to `visible/content` and its vertical
    position maps linearly to `ScrollY`. Dragging updates `ScrollY` while active.

## Renderer backend (Silk.NET OpenGL)

- Resources: one VAO + VBO (vertex) + EBO (index) allocated and resized with a
  power‑of‑two strategy as needed. A default 1×1 white texture avoids sampling
  unbound units for solid fills.

- Shaders: a minimal program with orthographic projection, vertex color, and UV.

- Per frame
  - Set blending, scissor, and bind program/VAO.
  - Upload concatenated vertex/index buffers with `BufferSubData`.
  - For each draw command: set scissor from `ClipRect`, bind `TextureId` (or
    white texture if zero), then issue `DrawElementsBaseVertex`.
  - Clear is performed with scissor disabled to avoid partial clears.

## Differences vs. Dear ImGui (today)

- Partial widget set (Button, Text/Label). Checkbox/Slider and text input are
  planned next.
- No docking/tables/menus yet.
- Fonts: ASCII subset baked by default; kerning and wider ranges will be added.
- Window identity is name‑based; this will evolve to include the ID stack to
  avoid collisions.

## Extensibility

- Renderers: the backend is isolated behind the `IRenderPipeline` interface.
  Additional backends (D3D/Vulkan/Metal) can implement the same contract.
- Textures: the OpenGL pipeline exposes `RegisterTexture/UnregisterTexture` to
  upload external RGBA images and reference them via `TextureId`.

## Testing guidance

- Unit tests: layout math (cursor/padding/scrolling), widget state transitions,
  and text measurement baselines.
- Rendering contract: verify draw‑command sequences (clip/texture ids) and a
  non‑blank off‑screen sanity render.

