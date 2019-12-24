namespace mprCopySheetsToOpenDocuments.Models
{
    using Autodesk.Revit.DB;
    using JetBrains.Annotations;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Модель листа для представления в окне плагина
    /// </summary>
    public class BrowserSheet : VmBase, IBrowserItem
    {
        private bool _checked;
        private string _sheetNumber;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="sheetName">Имя листа</param>
        /// <param name="sheetNumber">Номер листа</param>
        /// <param name="id">Идентификатор элемента Revit</param>
        /// <param name="parentGroup">Родительская группа</param>
        public BrowserSheet(string sheetName, string sheetNumber, ElementId id, [NotNull] BrowserSheetGroup parentGroup)
        {
            SheetName = sheetName;
            SheetNumber = sheetNumber;
            InitSheetNumber = sheetNumber;
            Id = id;
            ParentGroup = parentGroup;
        }

        /// <summary>
        /// Родительская группа
        /// </summary>
        public BrowserSheetGroup ParentGroup { get; }

        /// <summary>
        /// Имя листа
        /// </summary>
        public string SheetName { get; }

        /// <summary>
        /// Номер листа
        /// </summary>
        public string SheetNumber
        {
            get => _sheetNumber;
            set
            {
                _sheetNumber = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Номер листа при инициализации (номер, который был изначально).
        /// Свойство должно меняться при перенумерации перемещением!
        /// </summary>
        public string InitSheetNumber { get; set; }

        /// <summary>
        /// Идентификатор листа
        /// </summary>
        public ElementId Id { get; }

        /// <summary>Выбран ли лист</summary>
        public bool Checked
        {
            get => _checked;
            set
            {
                if (value == _checked) 
                    return;
                _checked = value;
                OnPropertyChanged();
            }
        }
    }
}
