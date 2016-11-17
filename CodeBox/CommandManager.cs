using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBox.ObjectModel;
using CodeBox.Commands;
using CodeBox.ComponentModel;
using CodeBox.Core;
using System.IO;

namespace CodeBox
{
    public sealed class CommandManager
    {
        private readonly Editor editor;
        private readonly KeyboardAdapter adapter;

        internal CommandManager(Editor editor)
        {
            this.editor = editor;
            adapter = new KeyboardAdapter();
        }

        public void ReadKeymap(string fileName)
        {
            var src = File.ReadAllText(fileName);
            KeymapReader.Read(src, adapter);
        }

        public void Run(KeyInput input)
        {
            Console.WriteLine($"KeyInput: {input}.");

            if (adapter.ProcessInput(input) == InputState.Complete)
            {
                var cmd = ComponentCatalog.Instance.GetComponent<IEditorCommand>(adapter.LastKey);
                if (cmd != null)
                    cmd.Clone().Run(editor);
            }
        }
    }
}
