using System;

class ShortcutMessage
{
    public ShortcutActions Action { get; set; } = ShortcutActions.None;
    public ShortcutActionsParameters Parameter { get; set; } = ShortcutActionsParameters.None;
    public Type RecipientType { get; set; } = null;
    public int RecipientIndex { get; set; } = -1;
}