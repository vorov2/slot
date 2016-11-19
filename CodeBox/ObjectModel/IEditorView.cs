using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ObjectModel
{
    public interface IEditorView
    {
        void Redraw(DocumentBuffer buffer);

        Brush CreateBrush(Color color);

        Pen CreatePen(Color color);

        Font CreateFont(FontStyle style);

        Font CreateSmallFont(FontStyle style);

        void Restyle(bool complete = false);

        bool UpdateVisibleRectangle(DocumentBuffer buffer);

        void SetScrollPositionX(int value);

        void SetScrollPositionY(int value);

        void ScrollX(int times);

        void ScrollY(int times);

        void InvalidateLines(DocumentBuffer buffer, InvalidateFlags flags = InvalidateFlags.None);

        int FirstVisibleLine { get; }

        int LastVisibleLine { get; }

        int FirstEditLine { get; set; }

        int LastEditLine { get; set; }

        Point ScrollPosition { get; }

        Size ScrollBounds { get; }

        EditorInfo Info { get; }

        EditorSettings Settings { get; }

        Pos Caret { get; }

        bool WordWrap { get; }

        int WordWrapColumn { get; }

        bool UseTabs { get; }

        int IndentSize { get; }

        bool ShowEol { get; }

        bool ShowWhitespace { get; }

        bool ShowLineLength { get; }

        bool CurrentLineIndicator { get; }
    }
}
