using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using static CodeBox.Commands.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Silent)]
    public sealed class CopyCommand : Command
    {
        public override bool Execute(CommandArgument arg, Selection sel)
        {
            var str = GetTextRange(Context, sel);
            Clipboard.SetText(str, TextDataFormat.UnicodeText);
            return true;
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
    }
}
