using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiSharp.Rendering;
using Silk.NET.OpenGL;

namespace ImGuiSharp.Rendering.SilkNet;

/// <summary>
/// Silk.NET-backed implementation of the ImGui render pipeline.
/// </summary>
public sealed unsafe class SilkNetRenderPipeline : IRenderPipeline
{
    private const string VertexSource = "#version 330 core\n" +
        "layout (location = 0) in vec2 aPos;\n" +
        "layout (location = 1) in vec2 aUV;\n" +
        "layout (location = 2) in vec4 aColor;\n" +
        "uniform mat4 uProjection;\n" +
        "out vec2 vUV;\n" +
        "out vec4 vColor;\n" +
        "void main()\n" +
        "{\n" +
        "    vUV = aUV;\n" +
        "    vColor = aColor;\n" +
        "    gl_Position = uProjection * vec4(aPos, 0.0, 1.0);\n" +
        "}\n";

    private const string FragmentSource = "#version 330 core\n" +
        "in vec2 vUV;\n" +
        "in vec4 vColor;\n" +
        "uniform sampler2D uTexture;\n" +
        "out vec4 FragColor;\n" +
        "void main()\n" +
        "{\n" +
        "    vec4 texColor = texture(uTexture, vUV);\n" +
        "    FragColor = vColor * texColor;\n" +
        "}\n";

    private readonly GL _gl;
    private bool _disposed;
    private bool _resourcesCreated;

    private uint _vertexArray;
    private uint _vertexBuffer;
    private uint _indexBuffer;
    private uint _vertexShader;
    private uint _fragmentShader;
    private uint _shaderProgram;
    private int _projectionLocation;
    private uint _defaultTexture;

    private int _vertexBufferCapacity;
    private int _indexBufferCapacity;

    /// <summary>
    /// Initializes a new instance of the <see cref="SilkNetRenderPipeline"/> class.
    /// </summary>
    /// <param name="gl">The OpenGL bindings used to issue GPU commands.</param>
    public SilkNetRenderPipeline(GL gl)
    {
        _gl = gl;
    }

    /// <inheritdoc />
    public void BeginFrame()
    {
        ThrowIfDisposed();
        EnsureResources();

        _gl.BindVertexArray(_vertexArray);
        _gl.UseProgram(_shaderProgram);

        _gl.Enable(GLEnum.Blend);
        _gl.BlendEquation(BlendEquationModeEXT.FuncAdd);
        _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        _gl.Disable(GLEnum.CullFace);
        _gl.Disable(GLEnum.DepthTest);
        _gl.Enable(GLEnum.ScissorTest);
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _defaultTexture);
    }

    /// <inheritdoc />
    public void Render(ImGuiDrawData drawData)
    {
        ThrowIfDisposed();
        EnsureResources();

        if (drawData.TotalVtxCount == 0 || drawData.TotalIdxCount == 0)
        {
            return;
        }

        var display = drawData.DisplayRect;
        SetProjection(display);
        UploadDrawData(drawData);

        float displayWidth = display.MaxX - display.MinX;
        float displayHeight = display.MaxY - display.MinY;
        if (displayWidth <= 0f || displayHeight <= 0f)
        {
            return;
        }

        _gl.Viewport(0, 0, (uint)displayWidth, (uint)displayHeight);

        var indexBase = 0;
        var vertexBase = 0;
        _gl.BindVertexArray(_vertexArray);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBuffer);

        foreach (var list in drawData.DrawLists)
        {
            var commandIndexOffset = 0;
            foreach (var command in list.Commands)
            {
                if (!ApplyClipRect(command.ClipRect, display))
                {
                    commandIndexOffset += command.ElementCount;
                    continue;
                }

                if (command.ElementCount <= 0)
                {
                    commandIndexOffset += command.ElementCount;
                    continue;
                }

                var indexPtr = (void*)((indexBase + commandIndexOffset) * sizeof(ushort));
                _gl.DrawElementsBaseVertex(PrimitiveType.Triangles, (uint)command.ElementCount, DrawElementsType.UnsignedShort, indexPtr, vertexBase);
                commandIndexOffset += command.ElementCount;
            }

            indexBase += list.Indices.Length;
            vertexBase += list.Vertices.Length;
        }
    }

    /// <inheritdoc />
    public void EndFrame()
    {
        ThrowIfDisposed();
        _gl.BindVertexArray(0);
        _gl.UseProgram(0);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_resourcesCreated)
        {
            _gl.DeleteBuffer(_vertexBuffer);
            _gl.DeleteBuffer(_indexBuffer);
            _gl.DeleteVertexArray(_vertexArray);
            _gl.DeleteProgram(_shaderProgram);
            _gl.DeleteShader(_vertexShader);
            _gl.DeleteShader(_fragmentShader);
            if (_defaultTexture != 0)
            {
                _gl.DeleteTexture(_defaultTexture);
            }
        }

        _disposed = true;
    }

    private void EnsureResources()
    {
        if (_resourcesCreated)
        {
            return;
        }

        _vertexArray = _gl.GenVertexArray();
        _vertexBuffer = _gl.GenBuffer();
        _indexBuffer = _gl.GenBuffer();

        _gl.BindVertexArray(_vertexArray);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBuffer);

        _vertexShader = CompileShader(ShaderType.VertexShader, VertexSource);
        _fragmentShader = CompileShader(ShaderType.FragmentShader, FragmentSource);
        _shaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_shaderProgram, _vertexShader);
        _gl.AttachShader(_shaderProgram, _fragmentShader);
        _gl.LinkProgram(_shaderProgram);
        ValidateProgram(_shaderProgram);

        _projectionLocation = _gl.GetUniformLocation(_shaderProgram, "uProjection");
        _gl.UseProgram(_shaderProgram);
        _gl.Uniform1(_gl.GetUniformLocation(_shaderProgram, "uTexture"), 0);

        var stride = (uint)ImGuiVertex.SizeInBytes;
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*)0);
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(2 * sizeof(float)));
        _gl.EnableVertexAttribArray(2);
        _gl.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, (void*)(4 * sizeof(float)));

        _gl.BindVertexArray(0);
        _resourcesCreated = true;

        // Create a default 1x1 white texture to avoid sampling an unbound unit
        _defaultTexture = _gl.GenTexture();
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _defaultTexture);
        Span<byte> pixel = stackalloc byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
        unsafe
        {
            fixed (byte* p = pixel)
            {
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, 1, 1, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, p);
            }
        }
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
    }

    private uint CompileShader(ShaderType type, string source)
    {
        var shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        var status = _gl.GetShader(shader, ShaderParameterName.CompileStatus);
        if (status == 0)
        {
            var info = _gl.GetShaderInfoLog(shader);
            _gl.DeleteShader(shader);
            throw new InvalidOperationException($"Failed to compile {type}: {info}");
        }

        return shader;
    }

    private void ValidateProgram(uint program)
    {
        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out var linkStatus);
        if (linkStatus == 0)
        {
            var info = _gl.GetProgramInfoLog(program);
            _gl.DeleteProgram(program);
            throw new InvalidOperationException($"Failed to link shader program: {info}");
        }
    }

    private void SetProjection(ImGuiRect displayRect)
    {
        float l = displayRect.MinX;
        float r = displayRect.MaxX;
        float t = displayRect.MinY;
        float b = displayRect.MaxY;

        Span<float> matrix = stackalloc float[16];
        matrix[0] = 2f / (r - l);
        matrix[5] = 2f / (t - b);
        matrix[10] = -1f;
        matrix[12] = (r + l) / (l - r);
        matrix[13] = (t + b) / (b - t);
        matrix[15] = 1f;

        _gl.UniformMatrix4(_projectionLocation, 1, false, in matrix[0]);
    }

    private void UploadDrawData(ImGuiDrawData drawData)
    {
        var totalVertexBytes = drawData.TotalVtxCount * ImGuiVertex.SizeInBytes;
        var totalIndexBytes = drawData.TotalIdxCount * sizeof(ushort);

        EnsureBufferCapacity(BufferTargetARB.ArrayBuffer, _vertexBuffer, ref _vertexBufferCapacity, totalVertexBytes);
        EnsureBufferCapacity(BufferTargetARB.ElementArrayBuffer, _indexBuffer, ref _indexBufferCapacity, totalIndexBytes);

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBuffer);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBuffer);

        var vertexOffsetBytes = 0;
        var indexOffsetBytes = 0;

        foreach (var list in drawData.DrawLists)
        {
            var vertices = list.Vertices.Span;
            var indices = list.Indices.Span;

            if (vertices.Length > 0)
            {
                var size = (nuint)(vertices.Length * ImGuiVertex.SizeInBytes);
                fixed (ImGuiVertex* vtxPtr = vertices)
                {
                    _gl.BufferSubData(BufferTargetARB.ArrayBuffer, (nint)vertexOffsetBytes, size, vtxPtr);
                }

                vertexOffsetBytes += (int)size;
            }

            if (indices.Length > 0)
            {
                var size = (nuint)(indices.Length * sizeof(ushort));
                fixed (ushort* idxPtr = indices)
                {
                    _gl.BufferSubData(BufferTargetARB.ElementArrayBuffer, (nint)indexOffsetBytes, size, idxPtr);
                }

                indexOffsetBytes += (int)size;
            }
        }
    }

    private void EnsureBufferCapacity(BufferTargetARB target, uint buffer, ref int currentCapacity, int requiredBytes)
    {
        if (requiredBytes <= currentCapacity)
        {
            return;
        }

        var newCapacity = NextCapacity(requiredBytes);
        _gl.BindBuffer(target, buffer);
        _gl.BufferData(target, (nuint)newCapacity, null, BufferUsageARB.DynamicDraw);
        currentCapacity = newCapacity;
    }

    private static int NextCapacity(int required)
    {
        var capacity = 1;
        while (capacity < required)
        {
            capacity <<= 1;
        }

        return capacity;
    }

    private bool ApplyClipRect(ImGuiRect clipRect, ImGuiRect displayRect)
    {
        var clipMinX = clipRect.MinX - displayRect.MinX;
        var clipMinY = clipRect.MinY - displayRect.MinY;
        var clipMaxX = clipRect.MaxX - displayRect.MinX;
        var clipMaxY = clipRect.MaxY - displayRect.MinY;

        var displayWidth = displayRect.MaxX - displayRect.MinX;
        var displayHeight = displayRect.MaxY - displayRect.MinY;

        clipMinX = System.MathF.Max(clipMinX, 0f);
        clipMinY = System.MathF.Max(clipMinY, 0f);
        clipMaxX = System.MathF.Min(clipMaxX, displayWidth);
        clipMaxY = System.MathF.Min(clipMaxY, displayHeight);

        var scissorX = (int)clipMinX;
        var scissorY = (int)(displayHeight - clipMaxY);
        var scissorW = (int)(clipMaxX - clipMinX);
        var scissorH = (int)(clipMaxY - clipMinY);

        if (scissorW <= 0 || scissorH <= 0)
        {
            return false;
        }

        _gl.Scissor(scissorX, scissorY, (uint)scissorW, (uint)scissorH);
        return true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SilkNetRenderPipeline));
        }
    }
}
