namespace mprCopySheetsToOpenDocuments.Models
{
    using Autodesk.Revit.DB;
    using ModPlusAPI.Mvvm;

    public class RevitDocument : VmBase
    {
        public RevitDocument(Document document)
        {
            Document = document;
        }

        public Document Document { get; }

        public string Title => Document.Title;

        private bool _selected;

        /// <summary>Документ выбран в списке</summary>
        public bool Selected
        {
            get => _selected;
            set
            {
                if (Equals(value, _selected)) return;
                _selected = value;
                OnPropertyChanged();
            }
        }
    }
}
