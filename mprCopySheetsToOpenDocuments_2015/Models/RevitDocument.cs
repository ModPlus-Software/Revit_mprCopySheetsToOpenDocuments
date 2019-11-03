namespace mprCopySheetsToOpenDocuments.Models
{
    using Autodesk.Revit.DB;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Модель документа Revit для выбора в окне плагина
    /// </summary>
    public class RevitDocument : VmBase
    {
        private bool _selected;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="document">Документ Revit</param>
        public RevitDocument(Document document)
        {
            Document = document;
        }

        /// <summary>
        /// Документ Revit
        /// </summary>
        public Document Document { get; }

        /// <summary>
        /// Название документа (заголовок)
        /// </summary>
        public string Title => Document.Title;
        
        /// <summary>
        /// Документ выбран в списке
        /// </summary>
        public bool Selected
        {
            get => _selected;
            set
            {
                if (Equals(value, _selected))
                    return;
                _selected = value;
                OnPropertyChanged();
            }
        }
    }
}
