using System;
using SMath = System.Math;

namespace ImGuiSharp;

/// <summary>
/// Tracks the editing state for the active InputText widget.
/// </summary>
internal sealed class ImGuiInputTextState
{
    public uint Id { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public string InitialText { get; private set; } = string.Empty;
    public int Cursor { get; private set; }
    public int SelectionStart { get; private set; }
    public int SelectionEnd { get; private set; }
    public float ScrollX { get; set; }

    public bool IsActive => Id != 0;
    public bool HasSelection => SelectionStart != SelectionEnd;

    public void Activate(uint id, string text, bool selectAll)
    {
        Id = id;
        Text = text ?? string.Empty;
        InitialText = Text;
        Cursor = Text.Length;
        if (selectAll && Text.Length > 0)
        {
            SelectionStart = 0;
            SelectionEnd = Text.Length;
        }
        else
        {
            SelectionStart = Cursor;
            SelectionEnd = Cursor;
        }
        ScrollX = 0f;
    }

    public void Deactivate()
    {
        Id = 0;
        Text = string.Empty;
        InitialText = string.Empty;
        Cursor = 0;
        SelectionStart = 0;
        SelectionEnd = 0;
        ScrollX = 0f;
    }

    public void SetText(string text)
    {
        Text = text ?? string.Empty;
        Cursor = SMath.Clamp(Cursor, 0, Text.Length);
        SelectionStart = SMath.Clamp(SelectionStart, 0, Text.Length);
        SelectionEnd = SMath.Clamp(SelectionEnd, 0, Text.Length);
    }

    public void ClearSelection()
    {
        SelectionStart = Cursor;
        SelectionEnd = Cursor;
    }

    public void SelectAll()
    {
        SelectionStart = 0;
        SelectionEnd = Text.Length;
        Cursor = Text.Length;
    }

    public void SetCursor(int position, bool extendSelection)
    {
        position = SMath.Clamp(position, 0, Text.Length);
        Cursor = position;
        if (extendSelection)
        {
            SelectionEnd = position;
        }
        else
        {
            SelectionStart = position;
            SelectionEnd = position;
        }
    }

    public bool DeleteSelection()
    {
        if (!HasSelection)
        {
            return false;
        }

        int start = SMath.Min(SelectionStart, SelectionEnd);
        int length = SMath.Abs(SelectionEnd - SelectionStart);
        Text = Text.Remove(start, length);
        Cursor = start;
        SelectionStart = Cursor;
        SelectionEnd = Cursor;
        return true;
    }

    public bool InsertText(string insert, int maxLength)
    {
        if (string.IsNullOrEmpty(insert))
        {
            return false;
        }

        int effectiveMax = maxLength <= 0 ? int.MaxValue : maxLength;
        int available = effectiveMax - Text.Length;
        if (HasSelection)
        {
            available += SMath.Abs(SelectionEnd - SelectionStart);
        }
        if (available <= 0)
        {
            return false;
        }

        if (insert.Length > available)
        {
            insert = insert.Substring(0, available);
        }

        bool modified = DeleteSelection();
        Text = Text.Insert(Cursor, insert);
        Cursor += insert.Length;
        SelectionStart = Cursor;
        SelectionEnd = Cursor;
        return modified || insert.Length > 0;
    }

    public bool DeleteBackspace(bool deleteWord)
    {
        if (DeleteSelection())
        {
            return true;
        }

        if (Cursor <= 0)
        {
            return false;
        }

        int start;
        if (deleteWord)
        {
            start = FindPrevWordBoundary(Cursor);
        }
        else
        {
            start = Cursor - 1;
        }
        start = SMath.Clamp(start, 0, Cursor);
        int deleteCount = Cursor - start;
        Text = Text.Remove(start, deleteCount);
        Cursor = start;
        SelectionStart = Cursor;
        SelectionEnd = Cursor;
        return true;
    }

    public bool DeleteForward(bool deleteWord)
    {
        if (DeleteSelection())
        {
            return true;
        }

        if (Cursor >= Text.Length)
        {
            return false;
        }

        int deleteCount;
        if (deleteWord)
        {
            int target = FindNextWordBoundary(Cursor);
            deleteCount = SMath.Max(1, target - Cursor);
        }
        else
        {
            deleteCount = 1;
        }

        Text = Text.Remove(Cursor, SMath.Min(deleteCount, Text.Length - Cursor));
        SelectionStart = Cursor;
        SelectionEnd = Cursor;
        return true;
    }

    public void MoveCursor(int delta, bool extendSelection)
    {
        SetCursor(Cursor + delta, extendSelection);
    }

    public void MoveCursorTo(int position, bool extendSelection)
    {
        SetCursor(position, extendSelection);
    }

    public int FindPrevWordBoundary(int from)
    {
        int pos = SMath.Clamp(from, 0, Text.Length);
        if (pos == 0)
        {
            return 0;
        }

        pos--;
        while (pos > 0 && char.IsWhiteSpace(Text[pos]))
        {
            pos--;
        }
        while (pos > 0 && !char.IsWhiteSpace(Text[pos - 1]))
        {
            pos--;
        }
        return pos;
    }

    public int FindNextWordBoundary(int from)
    {
        int pos = SMath.Clamp(from, 0, Text.Length);
        while (pos < Text.Length && !char.IsWhiteSpace(Text[pos]))
        {
            pos++;
        }
        while (pos < Text.Length && char.IsWhiteSpace(Text[pos]))
        {
            pos++;
        }
        return pos;
    }
}
