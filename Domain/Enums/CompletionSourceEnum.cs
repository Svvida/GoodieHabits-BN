namespace Domain.Enums
{
    /// <summary>Where a completion was recorded from. Stored so an imported tap can never be mistaken for one
    /// the user made in the app, and so a later calendar sync can reconcile its own writes.</summary>
    public enum CompletionSourceEnum
    {
        App,
        Widget,
        Calendar,
        Import
    }
}
