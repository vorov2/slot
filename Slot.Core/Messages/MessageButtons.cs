using System;

namespace Slot.Core.Messages
{
    [Flags]
    public enum MessageButtons
    {
        None = 0xFF,

        [FieldName("&OK")]
        Ok = 0x01,

        [FieldName("&Save")]
        Save = 0x02,

        [FieldName("&Save All")]
        SaveAll = 0x04,

        [FieldName("&Don't Save")]
        DontSave = 0x08,

        [FieldName("&Yes")]
        Yes = 0x10,

        [FieldName("&No")]
        No = 0x20,

        [FieldName("&Cancel")]
        Cancel = 0x40,

        [FieldName("&Close")]
        Close = 0x80
    }
}
