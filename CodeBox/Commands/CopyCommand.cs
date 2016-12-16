using System;
using System.Text;
using System.Windows.Forms;
using Slot.Editor.ObjectModel;
using static Slot.Editor.Commands.ActionResults;
using System.ComponentModel.Composition;
using Slot.Core.ComponentModel;

namespace Slot.Editor.Commands
{
    [Export(typeof(EditorCommand))]
    [ComponentData("editor.copy")]
    public sealed class CopyCommand : EditorCommand
    {
        internal override ActionResults Execute(Selection sel, params object[] args)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < Buffer.Selections.Count; i++)
            {
                var s = Buffer.Selections[i];
                var str = GetTextRange(View, s);
                sb.Append(str);
                if (i != Buffer.Selections.Count - 1)
                    sb.Append(Buffer.Eol.AsString());
            }

            if (sb.Length > 0)
                Clipboard.SetText(sb.ToString(), TextDataFormat.UnicodeText);

            return Pure;
        }

        internal static string GetTextRange(EditorControl ctx, Range range)
        {
            var doc = ctx.Buffer.Document;
            var sel = range.Normalize();

            if (sel.IsEmpty)
                return null;

            var str = "";

            if (sel.Start.Line == sel.End.Line)
            {
                str = doc.Lines[sel.Start.Line]
                    .GetRange(sel.Start.Col, sel.End.Col - sel.Start.Col)
                    .MakeString(ctx.Buffer.Eol);
            }
            else
            {
                var sb = new StringBuilder();
                var startLine = doc.Lines[sel.Start.Line];
                var endLine = doc.Lines[sel.End.Line];
                sb.Append(startLine
                    .GetRange(sel.Start.Col, startLine.Length - sel.Start.Col)
                    .MakeString(ctx.Buffer.Eol));
                var len = endLine.Length - sel.End.Col;

                if (sel.End.Line - sel.Start.Line > 0)
                {
                    sb.AppendLine();

                    for (var i = sel.Start.Line + 1; i < sel.End.Line; i++)
                        sb.AppendLine(doc.Lines[i].MakeString(ctx.Buffer.Eol));

                    sb.Append(endLine.GetRange(0, sel.End.Col).MakeString(ctx.Buffer.Eol));
                }

                str = sb.ToString();
            }

            return str;
        }

        internal override bool SingleRun => true;

        internal override bool SupportLimitedMode => true;
    }
}
