using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using static CodeBox.ObjectModel.ActionExponent;

namespace CodeBox.Commands
{
    [CommandBehavior(Silent)]
    internal sealed class CopyCommand : Command
    {
        public override void Execute(EditorContext context, Selection sel)
        {
            var str = GetTextRange(context, sel);
            Clipboard.SetText(str, TextDataFormat.UnicodeText);
        }

        internal static string GetTextRange(EditorContext ctx, Range rangesr)
        {
            var doc = ctx.Document;
            var sel = rangesr.Normalize();

            if (sel.IsEmpty)
                return null;

            var str = "";

            if (sel.Start.Line == sel.End.Line)
            {
                str = doc.Lines[sel.Start.Line]
                    .GetRange(sel.Start.Col, sel.End.Col - sel.Start.Col)
                    .MakeString(ctx.Eol);
            }
            else
            {
                var sb = new StringBuilder();
                var startLine = doc.Lines[sel.Start.Line];
                var endLine = doc.Lines[sel.End.Line];
                sb.Append(startLine
                    .GetRange(sel.Start.Col, startLine.Length - sel.Start.Col)
                    .MakeString(ctx.Eol));
                var len = endLine.Length - sel.End.Col;

                if (sel.End.Line - sel.Start.Line > 0)
                {
                    sb.AppendLine();

                    for (var i = sel.Start.Line + 1; i < sel.End.Line; i++)
                        sb.AppendLine(doc.Lines[i].MakeString(ctx.Eol));

                    sb.Append(endLine.GetRange(0, sel.End.Col).MakeString(ctx.Eol));
                }

                str = sb.ToString();
            }

            return str;
        }
    }
}
