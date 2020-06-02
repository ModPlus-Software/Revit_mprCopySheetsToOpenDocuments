namespace mprCopySheetsToOpenDocuments.Models
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using JetBrains.Annotations;
    using ModPlusAPI.IO;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Модель группы листов
    /// </summary>
    public class BrowserSheetGroup : VmBase, IBrowserItem
    {
        private bool _checked;
        private bool _isExpanded;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name">Имя группы</param>
        /// <param name="parentGroup">Родительская группа</param>
        /// <param name="level">Уровень группы в дереве</param>
        public BrowserSheetGroup(string name, [CanBeNull] BrowserSheetGroup parentGroup, int level)
        {
            Name = name;
            ParentGroup = parentGroup;
            Level = level;
            SubItems = new ObservableCollection<IBrowserItem>();
        }

        /// <summary>
        /// Имя группы
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Родительская группа
        /// </summary>
        [CanBeNull]
        public BrowserSheetGroup ParentGroup { get; }

        /// <summary>
        /// Уровень группы в дереве
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Листы в группе
        /// </summary>
        public ObservableCollection<IBrowserItem> SubItems { get; private set; }

        /// <summary>Выбраны ли все листы в группе</summary>
        public bool Checked
        {
            get => _checked;
            set
            {
                if (value == _checked)
                    return;
                _checked = value;
                foreach (var browserSheet in SubItems)
                    browserSheet.Checked = value;

                OnPropertyChanged();
            }
        }

        /// <summary>Развернута ли группа</summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Сортировать листы в группе
        /// </summary>
        /// <param name="sortOrder">Порядок сортировки</param>
        public void SortSheets(SortOrder sortOrder)
        {
            if (SubItems.Any())
            {
                if (SubItems.First() is BrowserSheet)
                {
                    var sheets = SubItems.Where(i => i is BrowserSheet).Cast<BrowserSheet>();
                    SubItems = sortOrder == SortOrder.Ascending
                        ? new ObservableCollection<IBrowserItem>(sheets.OrderBy(s => s.SheetNumber, new OrdinalStringComparer()))
                        : new ObservableCollection<IBrowserItem>(sheets.OrderByDescending(s => s.SheetNumber, new OrdinalStringComparer()));
                }
                else
                {
                    var groups = SubItems.Where(i => i is BrowserSheetGroup).Cast<BrowserSheetGroup>().ToList();
                    groups.ForEach(g => g.SortSheets(sortOrder));
                    SubItems = sortOrder == SortOrder.Ascending
                        ? new ObservableCollection<IBrowserItem>(groups.OrderBy(g => g.Name, new OrdinalStringComparer()))
                        : new ObservableCollection<IBrowserItem>(groups.OrderByDescending(g => g.Name, new OrdinalStringComparer()));
                }
            }
        }
    }
}
