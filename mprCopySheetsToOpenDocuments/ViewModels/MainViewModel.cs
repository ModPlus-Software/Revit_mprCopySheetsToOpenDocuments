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
    using Helpers;
    using Models;
    using ModPlusAPI;
    using ModPlusAPI.Mvvm;
    using ModPlusAPI.Windows;
    using ModPlusStyle.Controls.Dialogs;
    using Views;

    /// <summary>
    /// Модель представления главного окна плагина
    /// </summary>
    public class MainViewModel : VmBase
    {
        private readonly string _langItem = ModPlusConnector.Instance.Name;
        private readonly MainWindow _mainWindow;
        private readonly UIApplication _uiApplication;
        private bool _isWork;
        private int _progressMaximum = 1;
        private string _progressText = string.Empty;
        private int _progressValue;
        private bool _copyGuideGrids;
        private bool _copySheetRevisions;
        private bool _copySchedules;
        private bool _copyTitleBlocks;
        private bool _copyDraftingView;
        private bool _copyImageView;
        private bool _copyTextNotes;
        private bool _updateExistingViewContents;
        private bool _copyGenericAnnotation;
        private bool _copyLegend;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="uiApplication"><see cref="UIApplication"/></param>
        /// <param name="mainWindow">Ссылка на окно</param>
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
        public ICommand CopySheetsCommand => new RelayCommandWithoutParameter(CopySheets);

        /// <summary>
        /// Текст прогресса
        /// </summary>
        public string ProgressText
        {
            get => _progressText;
            set
            {
                if (Equals(value, _progressText))
                    return;
                _progressText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Текущее значение прогресса
        /// </summary>
        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                if (Equals(value, _progressValue))
                    return;
                _progressValue = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Максимальное значение прогресса
        /// </summary>
        public int ProgressMaximum
        {
            get => _progressMaximum;
            set
            {
                if (Equals(value, _progressMaximum))
                    return;
                _progressMaximum = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True - работа начата
        /// </summary>
        public bool IsWork
        {
            get => _isWork;
            set
            {
                if (Equals(value, _isWork))
                    return;
                _isWork = value;
                OnPropertyChanged();
            }
        }

        #region Copy Options

        /// <summary>
        /// Копировать направляющую сетку
        /// </summary>
        public bool CopyGuideGrids
        {
            get => _copyGuideGrids;
            set
            {
                if (Equals(value, _copyGuideGrids))
                    return;
                _copyGuideGrids = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopyGuideGrids), value.ToString(), true);
            }
        }

        /// <summary>
        /// Копировать ревизии
        /// </summary>
        public bool CopySheetRevisions
        {
            get => _copySheetRevisions;
            set
            {
                if (Equals(value, _copySheetRevisions))
                    return;
                _copySheetRevisions = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopySheetRevisions), value.ToString(), true);
            }
        }

        /// <summary>
        /// Копировать спецификации
        /// </summary>
        public bool CopySchedules
        {
            get => _copySchedules;
            set
            {
                if (Equals(value, _copySchedules))
                    return;
                _copySchedules = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopySchedules), value.ToString(), true);
            }
        }

        /// <summary>
        /// Копировать основные надписи
        /// </summary>
        public bool CopyTitleBlocks
        {
            get => _copyTitleBlocks;
            set
            {
                if (Equals(value, _copyTitleBlocks))
                    return;
                _copyTitleBlocks = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopyTitleBlocks), value.ToString(), true);
            }
        }

        /// <summary>
        /// Копировать изображения
        /// </summary>
        public bool CopyImageView
        {
            get => _copyImageView;
            set
            {
                if (Equals(value, _copyImageView))
                    return;
                _copyImageView = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopyImageView), value.ToString(), true);
            }
        }

        /// <summary>
        /// Копировать чертежные виды
        /// </summary>
        public bool CopyDraftingView
        {
            get => _copyDraftingView;
            set
            {
                if (Equals(value, _copyDraftingView))
                    return;
                _copyDraftingView = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopyDraftingView), value.ToString(), true);
            }
        }

        /// <summary>
        /// Копировать текст
        /// </summary>
        public bool CopyTextNotes
        {
            get => _copyTextNotes;
            set
            {
                if (Equals(value, _copyTextNotes))
                    return;
                _copyTextNotes = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopyTextNotes), value.ToString(), true);
            }
        }

        /// <summary>
        /// Обновить содержимое существующих видов
        /// </summary>
        public bool UpdateExistingViewContents
        {
            get => _updateExistingViewContents;
            set
            {
                if (Equals(value, _updateExistingViewContents))
                    return;
                _updateExistingViewContents = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(UpdateExistingViewContents), value.ToString(), true);
            }
        }

        /// <summary>
        /// Копировать аннотации
        /// </summary>
        public bool CopyGenericAnnotation
        {
            get => _copyGenericAnnotation;
            set
            {
                if (Equals(value, _copyGenericAnnotation))
                    return;
                _copyGenericAnnotation = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopyGenericAnnotation), value.ToString(), true);
            }
        }

        /// <summary>
        /// Копировать легенды
        /// </summary>
        public bool CopyLegend
        {
            get => _copyLegend;
            set
            {
                if (Equals(value, _copyLegend))
                    return;
                _copyLegend = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopyLegend), value.ToString(), true);
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
                            topGroup = new BrowserSheetGroup(folderItems.First().Name, null)
                            {
                                IsExpanded = true
                            };
                            browserSheetGroups.Add(topGroup);
                        }

                        var childGroups = new List<BrowserSheetGroup> { topGroup };
                        for (var i = 1; i < folderItems.Count; i++)
                        {
                            var childGroup = new BrowserSheetGroup(folderItems[i].Name, childGroups.Last())
                            {
                                IsExpanded = true
                            };
                            childGroups.Last().SubItems.Add(childGroup);
                            childGroups.Add(childGroup);
                            browserSheetGroups.Add(childGroup);
                        }

                        childGroups.Last().SubItems.Add(new BrowserSheet(sheet.Name, sheet.SheetNumber, sheetId, childGroups.Last()));
                    }
                }
                else
                {
                    notGroupingSheetIds.Add(sheetId);
                }
            }

            if (notGroupingSheetIds.Any())
            {
                var browserSheetGroup = new BrowserSheetGroup("???", null)
                {
                    IsExpanded = true
                };
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
                if (Equals(document, currentDoc))
                    continue;
                Documents.Add(new RevitDocument(document));
            }
        }

        private void LoadSettings()
        {
            CopyGuideGrids = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyGuideGrids)), out var b) && b;
            CopySheetRevisions = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopySheetRevisions)), out b) && b;
            CopySchedules = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopySchedules)), out b) && b;
            CopyLegend = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyLegend)), out b) && b;
            CopyTitleBlocks = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyTitleBlocks)), out b) && b;
            CopyImageView = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyImageView)), out b) && b;
            CopyDraftingView = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyDraftingView)), out b) && b;
            CopyTextNotes = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyTextNotes)), out b) && b;
            UpdateExistingViewContents = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(UpdateExistingViewContents)), out b) && b;
            CopyGenericAnnotation = bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyGenericAnnotation)), out b) && b;
        }

        private async void CopySheets()
        {
            var selectedSheets = GetSelectedSheets(SheetGroups).ToList();
            if (!selectedSheets.Any())
            {
                // Нужно выбрать листы для копирования!
                await _mainWindow.ShowMessageAsync(GetLangItem("m2"), string.Empty).ConfigureAwait(true);
                return;
            }

            var destinationDocuments = Documents.Where(d => d.Selected).ToList();

            if (!destinationDocuments.Any())
            {
                // Нужно выбрать целевые документы!
                await _mainWindow.ShowMessageAsync(GetLangItem("m3"), string.Empty).ConfigureAwait(true);
                return;
            }

            ProgressMaximum = (selectedSheets.Count * destinationDocuments.Count) - 1;
            IsWork = true;

            var doc = _uiApplication.ActiveUIDocument.Document;
            var progressIndex = 0;

            var transactionName = Language.GetFunctionLocalName(ModPlusConnector.Instance.Name, ModPlusConnector.Instance.LName);

            foreach (var destinationDocument in destinationDocuments)
            {
                _mainWindow.Focus();
                _mainWindow.Activate();
                var destDoc = destinationDocument.Document;
                var ignoreLegends = false;

                using (var transactionGroup = new TransactionGroup(destDoc, transactionName))
                {
                    transactionGroup.Start();

                    if (CopyLegend)
                    {
                        var сollectorViewLegend = new FilteredElementCollector(destDoc).OfClass(typeof(View));
                        if (сollectorViewLegend.Cast<View>().All(x => x.ViewType != ViewType.Legend))
                        {
                            // Для копирования легенд в целевом документе "{0}" требуется создать хотя бы одну легенду
                            // Продолжить копирование листов для этого документа?
                            var dialogResult = await _mainWindow.ShowMessageAsync(
                                string.Format(GetLangItem("m4"), destDoc.Title),
                                GetLangItem("m5"),
                                MessageDialogStyle.AffirmativeAndNegative,
                                new MetroDialogSettings
                                {
                                    AffirmativeButtonText = GetLangItem("yes"),
                                    NegativeButtonText = GetLangItem("no")
                                }).ConfigureAwait(true);
                            if (dialogResult == MessageDialogResult.Affirmative)
                            {
                                ignoreLegends = true;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    foreach (var browserSheet in selectedSheets)
                    {
                        var sheet = doc.GetElement(browserSheet.Id) as ViewSheet;

                        // Каждую итерацию оборачиваем в try{} catch{} чтобы в случае ошибки не прерывалась работа
                        try
                        {
                            // сбор контента листов для копирования
                            var viewContents = new FilteredElementCollector(doc).OwnedByView(sheet.Id);
                            var viewContentsId = new List<ElementId>();
                            if (viewContents.Any())
                            {
                                foreach (var itemContent in viewContents)
                                {
                                    if (itemContent.Category != null)
                                    {
                                        if (CopyTextNotes && itemContent.Category.Id.IntegerValue == -2000300)
                                        {
                                            viewContentsId.Add(itemContent.Id);
                                        }

                                        if (CopyGenericAnnotation && itemContent.Category.Id.IntegerValue == -2000150)
                                        {
                                            viewContentsId.Add(itemContent.Id);
                                        }

                                        if (CopyTitleBlocks && itemContent.Category.Id.IntegerValue == -2000280)
                                        {
                                            viewContentsId.Add(itemContent.Id);
                                        }

                                        if (CopySchedules && itemContent.Category.Id.IntegerValue == -2000570)
                                        {
                                            viewContentsId.Add(itemContent.Id);
                                        }
                                    }
                                }
                            }

                            using (var tr = new Transaction(destDoc, "Create"))
                            {
                                await Task.Delay(100).ConfigureAwait(true);

                                // Копирование листа "{0}" в документ "{1}"
                                ProgressText = string.Format(
                                    GetLangItem("m6"), $"{browserSheet.SheetNumber} - {browserSheet.SheetName}", destinationDocument.Title);

                                var cpOptions = new CopyPasteOptions();
                                cpOptions.SetDuplicateTypeNamesHandler(new CopyUseDestination());

                                tr.Start();

                                var viewSheets = new FilteredElementCollector(destDoc).OfClass(typeof(ViewSheet));
                                var newNumber = UtilCopy.GetSuffixNumberViewSheet(viewSheets.ToList(), browserSheet.SheetNumber);

                                var newViewSheet = ViewSheet.Create(destDoc, ElementId.InvalidElementId);
                                if (newViewSheet != null)
                                {
                                    newViewSheet.get_Parameter(BuiltInParameter.SHEET_NAME).Set(browserSheet.SheetName);
                                    newViewSheet.get_Parameter(BuiltInParameter.SHEET_NUMBER).Set(newNumber);

                                    if (viewContentsId.Any())
                                    {
                                        ElementTransformUtils.CopyElements(sheet, viewContentsId, newViewSheet, null, cpOptions);
                                    }

                                    if (CopyLegend && !ignoreLegends)
                                    {
                                        await Task.Delay(100).ConfigureAwait(true);

                                        // Копирование легенд с листа "{0}" в документ "{1}"
                                        ProgressText = string.Format(
                                            GetLangItem("m7"),
                                            $"{browserSheet.SheetNumber} - {browserSheet.SheetName}",
                                            destinationDocument.Title);
                                        UtilCopy.CopyLegend(doc, sheet, newViewSheet, destDoc, cpOptions);
                                    }

                                    if (CopyGuideGrids)
                                    {
                                        await Task.Delay(100).ConfigureAwait(true);
                                        
                                        // Копирование сеток направляющих с листа "{0}" в документ "{1}"
                                        ProgressText = string.Format(
                                            GetLangItem("m8"),
                                            $"{browserSheet.SheetNumber} - {browserSheet.SheetName}",
                                            destinationDocument.Title);
                                        UtilCopy.CopyGuideGrids(doc, sheet, newViewSheet, destDoc, cpOptions);
                                    }

                                    if (CopyDraftingView)
                                    {
                                        await Task.Delay(100).ConfigureAwait(true);
                                        
                                        // Копирование чертежных видов с листа "{0}" в документ "{1}"
                                        ProgressText = string.Format(
                                            GetLangItem("m9"),
                                            $"{browserSheet.SheetNumber} - {browserSheet.SheetName}",
                                            destinationDocument.Title);
                                        UtilCopy.CopyDraftingView(doc, sheet, newViewSheet, destDoc, cpOptions);
                                    }

                                    if (CopyImageView)
                                    {
                                        await Task.Delay(100).ConfigureAwait(true);
                                        
                                        // Копирование изображений с листа "{0}" в документ "{1}"
                                        ProgressText = string.Format(
                                            GetLangItem("m10"),
                                            $"{browserSheet.SheetNumber} - {browserSheet.SheetName}",
                                            destinationDocument.Title);
                                        UtilCopy.CopyImageView(doc, sheet, newViewSheet, destDoc, cpOptions);
                                    }

                                    if (CopySheetRevisions)
                                    {
                                        await Task.Delay(100).ConfigureAwait(true);
                                        
                                        // Копирование изменений с листа "{0}" в документ "{1}"
                                        ProgressText = string.Format(
                                            GetLangItem("m11"),
                                            $"{browserSheet.SheetNumber} - {browserSheet.SheetName}",
                                            destinationDocument.Title);
                                        UtilCopy.CopySheetRevisions(doc, sheet, newViewSheet, destDoc);
                                    }
                                }

                                tr.Commit();
                            }

                            progressIndex++;
                            ProgressValue = progressIndex;
                        }
                        catch (Exception exception)
                        {
                            ExceptionBox.Show(exception);
                        }
                    }
                    
                    transactionGroup.Assimilate();
                }
            }

            await _mainWindow.ShowMessageAsync(GetLangItem("m12"), string.Empty);

            ClearProgress();
            IsWork = false;
        }

        private void ClearProgress()
        {
            ProgressMaximum = 1;
            ProgressValue = 0;
            ProgressText = string.Empty;
        }

        private static string GetLangItem(string key)
        {
            return Language.GetItem(ModPlusConnector.Instance.Name, key);
        }

        private IEnumerable<BrowserSheet> GetSelectedSheets(IEnumerable<BrowserSheetGroup> groups)
        {
            return groups.SelectMany(GetSelectedSheets);
        }

        private IEnumerable<BrowserSheet> GetSelectedSheets(BrowserSheetGroup group)
        {
            foreach (var item in group.SubItems)
            {
                if (item is BrowserSheet browserSheet && browserSheet.Checked)
                {
                    yield return browserSheet;
                }
                else if (item is BrowserSheetGroup subGroup)
                {
                    foreach (var sheet in GetSelectedSheets(subGroup))
                    {
                        yield return sheet;
                    }
                }
            }
        }
    }
}