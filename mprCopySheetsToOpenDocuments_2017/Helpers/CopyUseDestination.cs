namespace mprCopySheetsToOpenDocuments.Helpers
{
    using Autodesk.Revit.DB;

    class CopyUseDestination : IDuplicateTypeNamesHandler
    {
        public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)
        {
            return DuplicateTypeAction.UseDestinationTypes;
        }
    }
}
