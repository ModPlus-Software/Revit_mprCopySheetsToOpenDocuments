namespace mprCopySheetsToOpenDocuments.Helpers
{
    using Autodesk.Revit.DB;

    /// <inheritdoc />
    public class CopyUseDestination : IDuplicateTypeNamesHandler
    {
        /// <inheritdoc />
        public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)
        {
            return DuplicateTypeAction.UseDestinationTypes;
        }
    }
}
