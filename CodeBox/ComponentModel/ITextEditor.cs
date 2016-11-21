using CodeBox.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.ComponentModel
{
    public interface ITextEditor
    {
        string GetTextRange(Range range); //CopyCommand

        void DeleteRange(Range range); //DeleteRangeCommand

        //?
        void InsertText(Pos pos, string str); //InsertRangeCommand

        Range FindWord(Pos pos); //SelectWordCommand

        bool Undo();

        bool Redo();

        SelectionList Selections { get; }//?
    }
}
