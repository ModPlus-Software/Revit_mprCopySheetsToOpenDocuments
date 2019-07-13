namespace mprCopySheetsToOpenDocuments.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Models;
    using ModPlusAPI;
    using ModPlusAPI.Mvvm;
    using ModPlusAPI.Windows;
    using ModPlusStyle.Controls.Dialogs;
    using Views;

    public class MainViewModel : VmBase
    {
        private readonly string _langItem = ModPlusConnector.Instance.Name;
        private readonly MainWindow _mainWindow;
        private readonly UIApplication _uiApplication;
        private bool _isWork;
        private int _progressMaximum = 1;
        private string _progressText = string.Empty;
        private int _progressValue;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="uiApplication"></param>
        /// <param name="mainWindow"></param>
        public MainViewModel(UIApplication uiApplication, MainWindow mainWindow)
        {
            _uiApplication = uiApplication;
            _mainWindow = mainWindow;
            ReadSheetBrowser();
            GetDocuments();
            LoadSettings();
        }

        /// <summary>
        /// Группы листов для отображения всех листов группами
        /// </summary>
        public ObservableCollection<BrowserSheetGroup> SheetGroups { get; private set; } = new ObservableCollection<BrowserSheetGroup>();

        /// <summary>
        /// Документы
        /// </summary>
        public ObservableCollection<RevitDocument> Documents { get; } = new ObservableCollection<RevitDocument>();

        /// <summary>
        /// Команда копирования
        /// </summary>
        public ICommand CopySheetsCommand => new RelayCommand(CopySheets);

        /// <summary>Текст прогресса</summary>
        public string ProgressText
        {
            get => _progressText;
            set
            {
                if (Equals(value, _progressText)) return;
                _progressText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Текущее значение прогресса</summary>
        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                if (Equals(value, _progressValue)) return;
                _progressValue = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Максимальное значение прогресса</summary>
        public int ProgressMaximum
        {
            get => _progressMaximum;
            set
            {
                if (Equals(value, _progressMaximum)) return;
                _progressMaximum = value;
                OnPropertyChanged();
            }
        }

        /// <summary>True - работа начата</summary>
        public bool IsWork
        {
            get => _isWork;
            set
            {
                if (Equals(value, _isWork)) return;
                _isWork = value;
                OnPropertyChanged();
            }
        }

        #region Copy Options

        private bool _copyGuideGrids;

        /// <summary></summary>
        public bool CopyGuideGrids
        {
            get => _copyGuideGrids;
            set
            {
                if (Equals(value, _copyGuideGrids)) return;
                _copyGuideGrids = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopyGuideGrids), value.ToString(), true);
            }
        }

        private bool _copySheetRevisions;

        /// <summary></summary>
        public bool CopySheetRevisions
        {
            get => _copySheetRevisions;
            set
            {
                if (Equals(value, _copySheetRevisions)) return;
                _copySheetRevisions = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopySheetRevisions), value.ToString(), true);
            }
        }

        private bool _copySchedules;

        /// <summary></summary>
        public bool CopySchedules
        {
            get => _copySchedules;
            set
            {
                if (Equals(value, _copySchedules)) return;
                _copySchedules = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopySchedules), value.ToString(), true);
            }
        }

        private bool _copyTitleBlocks;

        /// <summary></summary>
        public bool CopyTitleBlocks
        {
            get => _copyTitleBlocks;
            set
            {
                if (Equals(value, _copyTitleBlocks)) return;
                _copyTitleBlocks = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopyTitleBlocks), value.ToString(), true);
            }
        }

        private bool _copyViewports;

        /// <summary></summary>
        public bool CopyViewports
        {
            get => _copyViewports;
            set
            {
                if (Equals(value, _copyViewports)) return;
                _copyViewports = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopyViewports), value.ToString(), true);
            }
        }

        private bool _copyTextNotes;

        /// <summary></summary>
        public bool CopyTextNotes
        {
            get => _copyTextNotes;
            set
            {
                if (Equals(value, _copyTextNotes)) return;
                _copyTextNotes = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopyTextNotes), value.ToString(), true);
            }
        }

        private bool _updateExistingViewContents;

        /// <summary></summary>
        public bool UpdateExistingViewContents
        {
            get => _updateExistingViewContents;
            set
            {
                if (Equals(value, _updateExistingViewContents)) return;
                _updateExistingViewContents = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(UpdateExistingViewContents), value.ToString(), true);
            }
        }

        #endregion

        private void ReadSheetBrowser()
        {
            var doc = _uiApplication.ActiveUIDocument.Document;
            var browserOrganization = BrowserOrganization.GetCurrentBrowserOrganizationForSheets(doc);
            var sortingOrder = browserOrganization.SortingOrder;
            var browserSheetGroups = new List<BrowserSheetGroup>();

            var sheetIds = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .WhereElementIsNotElementType()
                .ToElementIds()
                .ToList();

            // листы в браузере могут быть неорганизованны
            // сделаю коллекцию таких листов
            var notGroupingSheetIds = new List<ElementId>();

            foreach (var sheetId in sheetIds)
            {
                var sheet = (ViewSheet)doc.GetElement(sheetId);
                var folderItems = browserOrganization.GetFolderItems(sheetId);
                if (folderItems.Any())
                {
                    var fullPath = string.Join("*", folderItems.Select(fi => fi.Name));
                    var existGroup = browserSheetGroups.FirstOrDefault(g => g.GetFullPath() == fullPath);
                    if (existGroup != null)
                    {
                        existGroup.SubItems.Add(new BrowserSheet(sheet.Name, sheet.SheetNumber, sheetId, existGroup));
                    }
                    else
                    {
                        var topGroup = browserSheetGroups.FirstOrDefault(g => g.ParentGroup == null && folderItems.First().Name == g.Name);
                        if (topGroup == null)
                        {
                            topGroup = new BrowserSheetGroup(folderItems.First().Name, null);
                            browserSheetGroups.Add(topGroup);
                        }

                        var childGroups = new List<BrowserSheetGroup> { topGroup };
                        for (var i = 1; i < folderItems.Count; i++)
                        {
                            var childGroup = new BrowserSheetGroup(folderItems[i].Name, childGroups.Last());
                            childGroups.Last().SubItems.Add(childGroup);
                            childGroups.Add(childGroup);
                            browserSheetGroups.Add(childGroup);
                        }

                        childGroups.Last().SubItems.Add(new BrowserSheet(sheet.Name, sheet.SheetNumber, sheetId, childGroups.Last()));
                    }
                }
                else notGroupingSheetIds.Add(sheetId);
            }

            if (notGroupingSheetIds.Any())
            {
                var browserSheetGroup = new BrowserSheetGroup("???", null);
                foreach (var sheetId in notGroupingSheetIds)
                {
                    var sheet = (ViewSheet)doc.GetElement(sheetId);
                    browserSheetGroup.SubItems.Add(new BrowserSheet(sheet.Name, sheet.SheetNumber, sheetId, browserSheetGroup));
                }

                browserSheetGroups.Add(browserSheetGroup);
            }

            // Sort
            var topGroups = browserSheetGroups.Where(g => g.ParentGroup == null).ToList();
            if (sortingOrder == SortingOrder.Ascending)
            {
                foreach (var sheetGroup in browserSheetGroups)
                    sheetGroup.SortSheets(SortOrder.Ascending);
                SheetGroups =
                    new ObservableCollection<BrowserSheetGroup>(topGroups.OrderBy(g => g.Name));
            }
            else
            {
                foreach (var sheetGroup in browserSheetGroups)
                    sheetGroup.SortSheets(SortOrder.Descending);
                SheetGroups =
                    new ObservableCollection<BrowserSheetGroup>(topGroups.OrderByDescending(g => g.Name));
            }
        }

        private void GetDocuments()
        {
            var currentDoc = _uiApplication.ActiveUIDocument.Document;
            foreach (Document document in _uiApplication.Application.Documents)
            {
                if (Equals(document, currentDoc)) continue;
                Documents.Add(new RevitDocument(document));
            }
        }

        private void LoadSettings()
        {
            CopyGuideGrids = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyGuideGrids)), out var b) && b;
            CopySheetRevisions = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopySheetRevisions)), out b) && b;
            CopySchedules = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopySchedules)), out b) && b;
            CopyTitleBlocks = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyTitleBlocks)), out b) && b;
            CopyViewports = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyViewports)), out b) && b;
            CopyTextNotes = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyTextNotes)), out b) && b;
            UpdateExistingViewContents = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(UpdateExistingViewContents)), out b) && b;
        }

        private async void CopySheets(object o)
        {
            var selectedSheets = SheetGroups
                .SelectMany(g => g.SubItems.Where(i => i is BrowserSheet sheet && sheet.Checked).Cast<BrowserSheet>())
                .ToList();

            if (!selectedSheets.Any())
            {
                await _mainWindow.ShowMessageAsync("Нужно выбрать листы!", string.Empty).ConfigureAwait(true);
                return;
            }

            var destinationDocuments = Documents.Where(d => d.Selected).ToList();

            if (!destinationDocuments.Any())
            {
                await _mainWindow.ShowMessageAsync("Нужно выбрать документы!", string.Empty).ConfigureAwait(true);
                return;
            }

            ProgressMaximum = selectedSheets.Count * destinationDocuments.Count - 1;
            IsWork = true;

            var doc = _uiApplication.ActiveUIDocument.Document;
            var progressIndex = 0;

            foreach (var browserSheet in selectedSheets)
            {
                var sheet = doc.GetElement(browserSheet.Id) as ViewSheet;

                foreach (var destinationDocument in destinationDocuments)
                {
                    // Каждую итерацию оборачиваем в try{} catch{} чтобы в случае ошибки не прерывалась работа
                    try
                    {
                        // имитация работы
                        await Task.Delay(500).ConfigureAwait(true);
                        ProgressText = $"Copy sheet {browserSheet.SheetNumber}-{browserSheet.SheetName} to document {destinationDocument.Title}";
                        progressIndex++;
                        ProgressValue = progressIndex;
                    }
                    catch (Exception exception)
                    {
                        ExceptionBox.Show(exception);
                    }
                }
            }

            // clear progress
            ProgressMaximum = 1;
            ProgressValue = 0;
            ProgressText = string.Empty;

            IsWork = false;
        }
    }
}