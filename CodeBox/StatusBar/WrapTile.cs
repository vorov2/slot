using CodeBox.Core;
using CodeBox.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.StatusBar
{
    public sealed class WrapTile : StatusBarTile
    {
        private readonly Editor editor;

        public WrapTile(Editor editor) : base(TileAlignment.Right)
        {
            this.editor = editor;
        }

        public override string Text
        {
            get { return "WRAP"; }
            set { base.Text = value; }
        }

        protected internal override void PerformClick()
        {
            ComponentCatalog.Instance.RunCommand(editor, Cmd.ToggleWordWrap);
            base.PerformClick();
        }
    }
}
