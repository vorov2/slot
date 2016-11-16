using CodeBox.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeBox.Test
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

            ed.Styles.Selection.BackColor = ColorTranslator.FromHtml("#264F78");
            ed.Styles.Default.ForeColor = ColorTranslator.FromHtml("#DCDCDC");
            ed.Styles.Default.BackColor = ColorTranslator.FromHtml("#1E1E1E");
            ed.Settings.CurrentLineIndicator = false;
            ed.Settings.MatchBrackets = false;
            ed.Settings.ShowEol = false;
            ed.Settings.ShowLineLength = false;
            ed.Settings.ShowWhitespace = false;
            ed.Settings.WordWrap = false;
            BindCommands();
        }

        private void BindCommands()
        {
            ed.Commands.Bind<AutocompleteCommand>(Keys.Control | Keys.Space);
            ed.Commands.Bind<FollowLinkCommand>(MouseEvents.Click, Keys.Control);
            ed.Commands.Bind<DeleteWordBackCommand>(Keys.Control | Keys.Back);
            ed.Commands.Bind<DeleteWordCommand>(Keys.Control | Keys.Delete);
            ed.Commands.Bind<RedoCommand>(Keys.Control | Keys.Y);
            ed.Commands.Bind<UndoCommand>(Keys.Control | Keys.Z);
            ed.Commands.Bind<SelectAllCommand>(Keys.Control | Keys.A);
            ed.Commands.Bind<PasteCommand>(Keys.Control | Keys.V);
            ed.Commands.Bind<CutCommand>(Keys.Control | Keys.X);
            ed.Commands.Bind<CopyCommand>(Keys.Control | Keys.C);
            ed.Commands.Bind<SetCaretCommand>(MouseEvents.Click, Keys.Alt);
            ed.Commands.Bind<SetCaretCommand>(MouseEvents.Click, Keys.None);
            ed.Commands.Bind<NormalSelectCommand>(MouseEvents.Move | MouseEvents.Click, Keys.Control);
            ed.Commands.Bind<NormalSelectCommand>(MouseEvents.Move | MouseEvents.Click, Keys.None);
            ed.Commands.Bind<SelectWordCommand>(MouseEvents.DoubleClick, Keys.None);
            ed.Commands.Bind<ClearSelectionCommand>(Keys.Escape);
            ed.Commands.Bind<LeftCommand>(Keys.Left);
            ed.Commands.Bind<RightCommand>(Keys.Right);
            ed.Commands.Bind<HomeCommand>(Keys.Home);
            ed.Commands.Bind<EndCommand>(Keys.End);
            ed.Commands.Bind<DeleteBackCommand>(Keys.Back);
            ed.Commands.Bind<DeleteCommand>(Keys.Delete);
            ed.Commands.Bind<ExtendLeftCommand>(Keys.Shift | Keys.Left);
            ed.Commands.Bind<ExtendRightCommand>(Keys.Shift | Keys.Right);
            ed.Commands.Bind<ExtendEndCommand>(Keys.Shift | Keys.End);
            ed.Commands.Bind<ExtendHomeCommand>(Keys.Shift | Keys.Home);
            ed.Commands.Bind<WordLeftCommand>(Keys.Control | Keys.Left);
            ed.Commands.Bind<WordRightCommand>(Keys.Control | Keys.Right);
            ed.Commands.Bind<ExtendWordRightCommandCommand>(Keys.Control | Keys.Shift | Keys.Right);
            ed.Commands.Bind<ExtendWordLeftCommandCommand>(Keys.Control | Keys.Shift | Keys.Left);
            ed.Commands.Bind<OvertypeCommand>(Keys.Insert);
        }

    }
}
