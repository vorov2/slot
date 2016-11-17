using System;
using System.Text;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionResults;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.copy")]
    public sealed class CopyCommand : EditorCommand
    {
        public override ActionResults Execute(Selection sel)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < Buffer.Selections.Count; i++)
            {
                var s = Buffer.Selections[i];
                var str = GetTextRange(Context, s);
                sb.Append(str);
                if (i != Buffer.Selections.Count - 1)
                    sb.Append(Buffer.Eol.AsString());
            }

            if (sb.Length > 0)
                Clipboard.SetText(sb.ToString(), TextDataFormat.UnicodeText);

            return Pure;
        }

        internal static string GetTextRange(IEditorContext ctx, Range rangesr)
        {
            var doc = ctx.Buffer.Document;
            var sel = rangesr.Normalize();

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

        public override bool SingleRun => true;
    }
}
