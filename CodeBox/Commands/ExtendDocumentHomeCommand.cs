﻿using System;
using CodeBox.ObjectModel;
using CodeBox.ComponentModel;
using System.ComponentModel.Composition;
using CodeBox.Core.ComponentModel;

namespace CodeBox.Commands
{
    [Export(typeof(IComponent))]
    [ComponentData("command.editor.extenddocumenthome")]
    public sealed class ExtendDocumentHomeCommand : SelectionCommand
    {
        protected override Pos Select(Selection sel) => default(Pos);
    }
}
