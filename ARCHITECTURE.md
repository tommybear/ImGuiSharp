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
  - `ImGuiContext.AddText` sets the builder’s current texture to the font atlas,
    applies kerning between adjacent glyphs, and emits one quad per glyph using
    atlas UVs and baseline positioning.
  - Vertical metrics (ascent/line-height) come from the font’s v-metrics
    (stbtt_GetFontVMetrics scaled by pixel height) for crisper baseline.
  - `MeasureTextWidth` applies kerning and caches per-string widths for reuse.
  - `ImGui.Text` renders at the current cursor and advances by line height;
    `ImGui.Label` renders absolute text without affecting the cursor.
  - Wrapping: `PushTextWrapPos(wrapPosX)`/`PopTextWrapPos()` control wrapping; `wrapPosX == 0` wraps at the window’s content width. `ImGui.TextWrapped()` wraps using the current wrap position (auto if none set). `ImGui.CalcTextSize(text, wrapWidth=-1, hideAfterDoubleHash=false)` mirrors Dear ImGui semantics for measurement.

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
- Line helpers
  - `ImGui.NewLine` advances the cursor by current line height + spacing.
  - `ImGui.Separator` draws a 1px horizontal line across the content region and
    advances the cursor with spacing above/below, mirroring Dear ImGui.
  - `ImGui.SeparatorText` centers a label between two separator halves; when the
    text is hidden (`##`), it falls back to a plain separator.
- Style
  - `ImGuiStyle` exposes spacing/padding and a colour table mirroring Dear ImGui
    defaults (`Text`, `TextDisabled`, `FrameBg` variants, `Button` variants,
    `CheckMark`, `SliderGrab`, `NavHighlight`, etc.). Widgets source their
    colours from this palette and render focus borders using `NavHighlight`.
    `FrameBorderSize` controls optional borders drawn around frame widgets when
    non-zero, matching Dear ImGui's `style.FrameBorderSize`.
  - Style stacks (`PushStyleColor/Var` + `PopStyleColor/Var`) are implemented
    for the currently exposed slots, mirroring Dear ImGui's behaviour.
- Item queries
  - After each widget submission, `LastItemStatusFlags` tracks hover/active/press
    state. Public helpers mirror Dear ImGui: `ImGui.IsItemHovered/Active/Focused`
    and `ImGui.IsItemClicked`.
- Navigation
  - Basic keyboard navigation: Tab/Shift+Tab (and arrow keys) cycle focus across
    focusable widgets, updating `FocusedId` so `ImGui.IsItemFocused` reflects the
    current target. Focused widgets render a nav highlight border using the
    style colour. Full navigation/gamepad handling will follow.

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

- Early widget set (Button, Text/Label, Checkbox, SliderFloat). Text input and
  more complex widgets are planned next.

## Widget interaction notes

- Checkbox
  - ID derived from label (and ID stack when pushed) guards hover/active state.
  - Click press anywhere on the framed region (including the label) activates;
    release while hovered toggles the value.
  - Visuals: filled box with inner mark when checked; label drawn to the right;
    navigation focus draws a highlight border around the full item rect.
- RadioButton
  - Similar hover/active behaviour; selecting an option assigns the bound value.
  - Visuals: circular frame from `FrameBg` colours with inner dot using
    `CheckMark` colour when selected; nav focus highlights the whole row.

- Shared widget interaction uses an internal `ButtonBehavior` helper mirroring
  Dear ImGui, so Button/Checkbox/Radio/Slider all share the same hover/press
  handling for pointer input, reducing drift.

- SliderFloat
  - Press within the track activates; horizontal mouse drag maps pixels→value
    with clamping to `[min,max]` and returns `true` on release.
  - Visuals: track + knob; label and formatted value are drawn near the slider.
  - Focus highlight: keyboard focus draws a nav-coloured frame border around the
    slider body.
  - Keyboard: default step is `(max-min)/100`; `Shift` multiplies by 10,
    `Ctrl` divides by 10. An optional explicit step overrides the base step.
- No docking/tables/menus yet.
- Fonts: ASCII subset baked by default; kerning and wider ranges will be added.
- Window identity seeds item IDs: window IDs hash the window name combined with
  the parent window’s ID. Item IDs include the current window ID + user ID stack
  so identical labels in different windows do not collide.

## How To Add A New Widget

- ID and registration
  - Derive a stable ID from the label and current ID stack: `var id = context.GetId(label);`
  - Compute the item rect at `CursorPos` and call `context.RegisterItem(id, rect);` if applicable.

- Hit‑testing and state
  - `hovered = context.IsMouseHoveringRect(min, max);` → `context.SetHoveredId(id)` when hovered.
  - On press over the item: `context.SetActiveId(id)`; on release, commit the action if still hovered, then `context.ClearActiveId()`.
  - Return `true` when the widget’s bound value changes (or when committing a drag, like `SliderFloat`).

- Drawing
  - Use `context.AddRectFilled`/`ImGui.FillRect` for solid geometry. Solid fills assume `TextureId == 0` (renderer binds the 1×1 white texture).
  - Use `context.AddText(baseline, text, color)` for labels/values; baseline Y is `pos.Y + context.GetAscent()`.
  - Respect the current clip rect; the builder clips per draw command automatically.

- Layout
  - Advance vertical layout via `context.AdvanceCursor(new Vec2(0, height))` (height typically line height or control height + spacing).
  - For multi‑line controls, advance by the drawn height plus a small gap.

- ID stack and duplicates
  - Support duplicate labels by relying on `PushID/PopID` around repeated items in caller code.
  - Avoid hidden “##suffix” parsing for now; prefer explicit `PushID` scopes.

- Testing checklist
  - Hover/press/release transitions set `HoveredId`/`ActiveId` as expected and toggle/commit the value.
  - ID stability with `PushID` in repeated rows (e.g., inside a child/scrolling region).
  - Cursor advancement equals control height; no overlap between consecutive items.
  - Draw‑list invariants: solid fills emit `TextureId == 0`; text quads use the atlas texture and current clip rect.
  - For sliders/drags: pixel→value mapping is clamped and monotonic; returns `true` on commit.

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

## Tooling

- Editors: VS Code launch/tasks are provided under `.vscode.sample/`. Copy to `.vscode/` locally if you want to debug the sample (`coreclr` launch) or run the test suite via tasks.
