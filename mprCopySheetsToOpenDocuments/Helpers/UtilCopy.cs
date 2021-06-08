namespace mprCopySheetsToOpenDocuments.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Autodesk.Revit.DB;
    using ModPlusAPI;
    using ModPlusStyle.Controls.Dialogs;
    using Views;

    /// <summary>
    /// Методы копирования видов
    /// </summary>
    public static class UtilCopy
    {
        /// <summary>
        /// Копировать чертежный вид
        /// </summary>
        /// <param name="mainWindow">Ссылка на окно главное окно</param>
        /// <param name="activeDocument">Активный документ</param>
        /// <param name="sheet">Лист</param>
        /// <param name="sheetNew">Новый лист</param>
        /// <param name="destinationDocument">Документ назначения</param>
        /// <param name="cpOptions">Опции копирования</param>
        public static async Task CopyDraftingView(
            MainWindow mainWindow,
            Document activeDocument,
            ViewSheet sheet,
            ViewSheet sheetNew,
            Document destinationDocument,
            CopyPasteOptions cpOptions)
        {
            var viewPortsId = sheet.GetAllViewports();
            var viewContents = new List<ElementId>();
            var destinationDraftingViews = new FilteredElementCollector(destinationDocument).OfClass(typeof(ViewDrafting));
            if (viewPortsId.Any())
            {
                foreach (var viewPortId in viewPortsId)
                {
                    if (activeDocument.GetElement(viewPortId) is Viewport viewport)
                    {
                        var viewportItem = activeDocument.GetElement(viewport.ViewId);
                        if (viewportItem is ViewDrafting view && view.ViewType == ViewType.DraftingView)
                        {
                            Viewport newViewPort = null;

                            var copyDraftingView = true;
                            var existDestinationDraftingView =
                                destinationDraftingViews.FirstOrDefault(l => l.Name == view.Name);

                            if (existDestinationDraftingView != null && 
                                new FilteredElementCollector(destinationDocument)
                                    .OfClass(typeof(Viewport))
                                    .Cast<Viewport>().All(vp => vp.ViewId != existDestinationDraftingView.Id))
                            {
                                // В целевом документе "{0}" имеется чертежный вид "{1}"
                                var dialogResult = await mainWindow.ShowMessageAsync(
                                    string.Format(Language.GetItem(ModPlusConnector.Instance.Name, "m15"), destinationDocument.Title, existDestinationDraftingView.Name),
                                    Language.GetItem(ModPlusConnector.Instance.Name, "m16"),
                                    MessageDialogStyle.AffirmativeAndNegative,
                                    new MetroDialogSettings
                                    {
                                        AffirmativeButtonText = Language.GetItem(ModPlusConnector.Instance.Name, "exist"),
                                        NegativeButtonText = Language.GetItem(ModPlusConnector.Instance.Name, "copy")
                                    }).ConfigureAwait(true);
                                if (dialogResult == MessageDialogResult.Affirmative)
                                {
                                    copyDraftingView = false;
                                    newViewPort = Viewport.Create(
                                        destinationDocument, sheetNew.Id, existDestinationDraftingView.Id, viewport.GetBoxCenter());
                                }
                            }

                            if (copyDraftingView)
                            {
                                var newView = ViewDrafting.Create(destinationDocument, destinationDocument.GetDefaultElementTypeId(ElementTypeGroup.ViewTypeDrafting));
                                newView.Scale = view.Scale;
                                newView.Name = GetSuffixNameElements(destinationDraftingViews, view.Name);
                                var viewContentsDrafting =
                                    new FilteredElementCollector(activeDocument).OwnedByView(view.Id);

                                foreach (var item in viewContentsDrafting
                                    .Where(i => i.Category != null &&
                                                !i.Category.IsBuiltInCategory(BuiltInCategory.OST_IOSSketchGrid)))
                                {
                                    viewContents.Add(item.Id);
                                }

                                if (viewContents.Any())
                                {
                                    ElementTransformUtils.CopyElements(view, viewContents, newView, null, cpOptions);
                                }

                                newViewPort = Viewport.Create(destinationDocument, sheetNew.Id, newView.Id,
                                    viewport.GetBoxCenter());
                            }

                            if (newViewPort != null)
                            {
                                destinationDocument.Regenerate();
                                var searchText = activeDocument.GetElement(viewport.GetTypeId())
                                    .get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();

                                if (CheckElementName(BuiltInParameter.ALL_MODEL_TYPE_NAME, searchText, destinationDocument))
                                {
                                    var viewPortsTypeIds = ElementTransformUtils.CopyElements(
                                        activeDocument,
                                        new List<ElementId> { viewport.GetTypeId() },
                                        destinationDocument,
                                        null,
                                        cpOptions);
                                    newViewPort.ChangeTypeId(viewPortsTypeIds.First());
                                }
                                else
                                {
                                    var nameType = viewport.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM)
                                        .AsValueString();
                                    var viewPortType = FilterByName.FilterElementByNameEqualsCollector(
                                        BuiltInParameter.ALL_MODEL_TYPE_NAME,
                                        nameType,
                                        destinationDocument).OfClass(typeof(ElementType)).First().Id;
                                    newViewPort.ChangeTypeId(viewPortType);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Копировать виды изображений
        /// </summary>
        /// <param name="activeDocument">Активный документ</param>
        /// <param name="sheet">Лист</param>
        /// <param name="sheetNew">Новый лист</param>
        /// <param name="destinationDocument">Документ назначения</param>
        /// <param name="cpOptions">Опции копирования</param>
        public static void CopyImageView(
            Document activeDocument,
            ViewSheet sheet,
            ViewSheet sheetNew,
            Document destinationDocument,
            CopyPasteOptions cpOptions)
        {
            var viewPortsId = sheet.GetAllViewports();

            if (viewPortsId.Any())
            {
                foreach (var viewPortId in viewPortsId)
                {
                    if (activeDocument.GetElement(viewPortId) is Viewport viewport)
                    {
                        var viewportItem = activeDocument.GetElement(viewport.ViewId);
                        if (viewportItem is ImageView view && view.ViewType == ViewType.Rendering)
                        {
                            var imageViewId = ElementTransformUtils.CopyElements(
                                activeDocument,
                                new List<ElementId> { view.Id },
                                destinationDocument,
                                null,
                                cpOptions);
                            destinationDocument.Regenerate();
                            var searchText = activeDocument
                                .GetElement(viewport.GetTypeId())
                                .get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME)
                                .AsString();
                            var newViewPort = Viewport.Create(destinationDocument, sheetNew.Id, imageViewId.First(), viewport.GetBoxCenter());
                            if (CheckElementName(BuiltInParameter.ALL_MODEL_TYPE_NAME, searchText, destinationDocument))
                            {
                                var viewPortsTypeIds = ElementTransformUtils.CopyElements(
                                    activeDocument, new List<ElementId> { viewport.GetTypeId() }, destinationDocument, null, cpOptions);
                                newViewPort.ChangeTypeId(viewPortsTypeIds.First());
                            }
                            else
                            {
                                var nameType = viewport.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString();
                                var viewPortType = FilterByName.FilterElementByNameEqualsCollector(
                                    BuiltInParameter.ALL_MODEL_TYPE_NAME, nameType, destinationDocument)
                                    .OfClass(typeof(ElementType))
                                    .First().Id;
                                newViewPort.ChangeTypeId(viewPortType);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Копировать легенды
        /// </summary>
        /// <param name="mainWindow">Ссылка на окно главное окно</param>
        /// <param name="activeDocument">Активный документ</param>
        /// <param name="sheet">Лист</param>
        /// <param name="sheetNew">Новый лист</param>
        /// <param name="destinationDocument">Документ назначения</param>
        /// <param name="cpOptions">Опции копирования</param>
        /// <param name="copyRulesForAll">Пара булевых значений с условиями
        /// "Использовать существующую (для всех случаев)" и "Создать копию (для всех случаев)"</param>
        public static async Task<Tuple<bool, bool>> CopyLegend(
            MainWindow mainWindow,
            Document activeDocument,
            ViewSheet sheet,
            ViewSheet sheetNew,
            Document destinationDocument,
            CopyPasteOptions cpOptions,
            Tuple<bool, bool> copyRulesForAll)
        {
            var viewPortsId = sheet.GetAllViewports();

            var destinationViewLegends = new FilteredElementCollector(destinationDocument)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(x => x.ViewType == ViewType.Legend)
                .ToList();

            if (viewPortsId.Any())
            {
                foreach (var viewPortId in viewPortsId)
                {
                    if (activeDocument.GetElement(viewPortId) is Viewport viewport)
                    {
                        var viewportItem = activeDocument.GetElement(viewport.ViewId);
                        if (viewportItem is View viewLegend && viewLegend.ViewType == ViewType.Legend)
                        {
                            Viewport newViewPort = null;
                            var copyLegend = true;
                            var existDestinationLegend =
                                destinationViewLegends.FirstOrDefault(l => l.Name == viewLegend.Name);

                            if (existDestinationLegend != null)
                            {
                                if (copyRulesForAll.Item1)
                                {
                                    copyLegend = false;
                                    newViewPort = Viewport.Create(
                                        destinationDocument, sheetNew.Id, existDestinationLegend.Id, viewport.GetBoxCenter());
                                }
                                else if (!copyRulesForAll.Item2)
                                {
                                    // В целевом документе "{0}" имеется легенда "{1}"
                                    var dialogResult = await mainWindow.ShowMessageAsync(
                                        string.Format(
                                            Language.GetItem(ModPlusConnector.Instance.Name, "m13"),
                                            destinationDocument.Title, existDestinationLegend.Name),
                                        Language.GetItem(ModPlusConnector.Instance.Name, "m14"),
                                        MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary,
                                        new MetroDialogSettings
                                        {
                                            AffirmativeButtonText =
                                                Language.GetItem(ModPlusConnector.Instance.Name, "exist"),
                                            NegativeButtonText =
                                                Language.GetItem(ModPlusConnector.Instance.Name, "copy"),
                                            FirstAuxiliaryButtonText =
                                                Language.GetItem(ModPlusConnector.Instance.Name, "existAll"),
                                            SecondAuxiliaryButtonText =
                                                Language.GetItem(ModPlusConnector.Instance.Name, "copyAll"),
                                        }).ConfigureAwait(true);

                                    if (dialogResult == MessageDialogResult.Affirmative)
                                    {
                                        copyLegend = false;
                                        newViewPort = Viewport.Create(
                                            destinationDocument, sheetNew.Id, existDestinationLegend.Id,
                                            viewport.GetBoxCenter());
                                    }
                                    else if (dialogResult == MessageDialogResult.FirstAuxiliary)
                                    {
                                        copyLegend = false;
                                        newViewPort = Viewport.Create(
                                            destinationDocument, sheetNew.Id, existDestinationLegend.Id,
                                            viewport.GetBoxCenter());
                                        copyRulesForAll = new Tuple<bool, bool>(true, false);
                                    }
                                    else if (dialogResult == MessageDialogResult.SecondAuxiliary)
                                    {
                                        copyRulesForAll = new Tuple<bool, bool>(false, true);
                                    }
                                }
                            }

                            if (copyLegend)
                            {
                                var viewContents = new FilteredElementCollector(activeDocument)
                                    .OwnedByView(viewLegend.Id)
                                    .ToElementIds()
                                    .Where(x => 
                                        activeDocument.GetElement(x).Category != null &&
                                        !activeDocument.GetElement(x).Category.IsBuiltInCategory(BuiltInCategory.OST_IOSSketchGrid))
                                    .ToList();
                                if (viewContents.Any())
                                {
                                    if (GetValidLegend(destinationViewLegends) is View newViewLegend)
                                    {
                                        newViewLegend.Name = GetSuffixNameElements(destinationViewLegends, viewLegend.Name);
                                        newViewLegend.Scale = viewLegend.Scale;
                                        ElementTransformUtils.CopyElements(
                                            viewLegend, viewContents, newViewLegend, null, cpOptions);

                                        newViewPort = Viewport.Create(
                                            destinationDocument, sheetNew.Id, newViewLegend.Id, viewport.GetBoxCenter());
                                    }
                                }
                            }

                            if (newViewPort != null)
                            {
                                destinationDocument.Regenerate();
                                var searchText = activeDocument.GetElement(viewport.GetTypeId())
                                    .get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
                                if (CheckElementName(
                                    BuiltInParameter.ALL_MODEL_TYPE_NAME, searchText, destinationDocument))
                                {
                                    var viewPortsTypeIds = ElementTransformUtils.CopyElements(
                                        activeDocument,
                                        new List<ElementId> { viewport.GetTypeId() },
                                        destinationDocument, null,
                                        cpOptions);
                                    newViewPort.ChangeTypeId(viewPortsTypeIds.First());
                                }
                                else
                                {
                                    var nameType = viewport
                                        .get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM)
                                        .AsValueString();
                                    var viewPortType = FilterByName.FilterElementByNameEqualsCollector(
                                            BuiltInParameter.ALL_MODEL_TYPE_NAME, nameType, destinationDocument)
                                        .OfClass(typeof(ElementType)).First().Id;
                                    newViewPort.ChangeTypeId(viewPortType);
                                }
                            }
                        }
                    }
                }
            }

            return copyRulesForAll;
        }

        /// <summary>
        /// Копировать направляющую сетку
        /// </summary>
        /// <param name="activeDocument">Активный документ</param>
        /// <param name="sheet">Лист</param>
        /// <param name="sheetNew">Новый лист</param>
        /// <param name="destinationDocument">Документ назначения</param>
        /// <param name="cpOptions">Опции копирования</param>
        public static bool CopyGuideGrids(
            Document activeDocument,
            ViewSheet sheet,
            ViewSheet sheetNew,
            Document destinationDocument,
            CopyPasteOptions cpOptions)
        {
            var guideGridsId = sheet.get_Parameter(BuiltInParameter.SHEET_GUIDE_GRID).AsElementId();
            if (guideGridsId != ElementId.InvalidElementId)
            {
                var sheetGuideGridsId = new List<ElementId> { guideGridsId };
                ElementTransformUtils.CopyElements(activeDocument, sheetGuideGridsId, destinationDocument, null, cpOptions);
                var guideElements = new FilteredElementCollector(destinationDocument)
                    .OfCategory(BuiltInCategory.OST_GuideGrid)
                    .WhereElementIsNotElementType()
                    .Where(x => x.Name == activeDocument.GetElement(guideGridsId).Name)
                    .ToList();
                if (guideElements.Any())
                {
                    sheetNew.get_Parameter(BuiltInParameter.SHEET_GUIDE_GRID).Set(guideElements.First().Id);
                }
            }

            return true;
        }

        /// <summary>
        /// Копировать ревизии
        /// </summary>
        /// <param name="activeDocument">Активный документ</param>
        /// <param name="sheet">Лист</param>
        /// <param name="sheetNew">Новый лист</param>
        /// <param name="destinationDocument">Документ назначения</param>
        public static void CopySheetRevisions(Document activeDocument, ViewSheet sheet, ViewSheet sheetNew, Document destinationDocument)
        {
            var revisions = Revision.GetAllRevisionIds(destinationDocument)
                .Select(item => destinationDocument.GetElement(item) as Revision)
                .ToList();
            var sheetRevisions = sheet.GetAllRevisionIds().Select(x => activeDocument.GetElement(x) as Revision);
            ICollection<ElementId> sheetRevisionsNew = new List<ElementId>();
            foreach (var revision in sheetRevisions.Where(r => r != null))
            {
                if (revisions.All(item => item.Description != revision.Description))
                {
                    var newRevision = Revision.Create(destinationDocument);
                    newRevision.Description = revision.Description;
                    newRevision.IssuedBy = revision.IssuedBy;
                    newRevision.IssuedTo = revision.IssuedTo;
                    newRevision.NumberType = revision.NumberType;
                    newRevision.RevisionDate = revision.RevisionDate;
                    sheetRevisionsNew.Add(newRevision.Id);
                }
                else
                {
                    var desRevision = revisions.FirstOrDefault(item => item.Description == revision.Description);
                    if (desRevision != null)
                        sheetRevisionsNew.Add(desRevision.Id);
                }
            }

            sheetNew.SetAdditionalRevisionIds(sheetRevisionsNew);
        }

        /// <summary>
        /// Проверка наличия элемента по имени
        /// </summary>
        /// <param name="bip">BuiltIn id</param>
        /// <param name="searchText">Строка поиска</param>
        /// <param name="destinationDocument">Документ назначения</param>
        /// <returns></returns>
        public static bool CheckElementName(BuiltInParameter bip, string searchText, Document destinationDocument)
        {
            var element = FilterByName.FilterElementByNameEqualsCollector(bip, searchText, destinationDocument)
                .FirstOrDefault(x => x.Name == searchText);
            if (element == null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Is this <see cref="Category"/> match <see cref="BuiltInCategory"/>
        /// </summary>
        /// <param name="category"><see cref="Category"/></param>
        /// <param name="builtInCategory"><see cref="BuiltInCategory"/></param>
        public static bool IsBuiltInCategory(this Category category, BuiltInCategory builtInCategory)
        {
            return category.Id.IntegerValue == (int)builtInCategory;
        }
        
        /// <summary>
        /// Получает имя листа с совпавшим числовым суффиксом в номере
        /// </summary>
        /// <param name="elements">Коллекция элементов</param>
        /// <param name="name">Имя листа</param>
        public static string GetSuffixNumberViewSheet(IEnumerable<Element> elements, string name)
        {
            var existNames = elements
                .Select(item => item.get_Parameter(BuiltInParameter.SHEET_NUMBER)?.AsString() ?? string.Empty)
                .ToList();
            var index = 0;
            var newName = name;
            while (existNames.Contains(newName))
            {
                index++;
                newName = $"{name} Copy {index}";
            }

            return newName;
        }

        /// <summary>
        /// Получает имя листа с совпавшим числовым суффиксом в имени
        /// </summary>
        /// <param name="elements">Коллекция элементов</param>
        /// <param name="name">Имя листа</param>
        public static string GetSuffixNameElements(IEnumerable<Element> elements, string name)
        {
            var existNames = elements.Select(item => item.Name).ToList();
            var index = 0;
            var newName = name;
            while (existNames.Contains(newName))
            {
                index++;
                newName = $"{name} Copy {index}";
            }

            return newName;
        }
        
        private static View GetValidLegend(IEnumerable<View> legends)
        {
            Exception cachedLastException = null;
            foreach (var legend in legends)
            {
                try
                {
                    using (var tr = new SubTransaction(legend.Document))
                    {
                        tr.Start();
                        legend.Document.GetElement(legend.Duplicate(ViewDuplicateOption.Duplicate));
                        tr.RollBack();
                    }

                    return legend;
                }
                catch (Exception exception)
                {
                    cachedLastException = exception;
                    
                    // go to next
                }
            }

            if (cachedLastException != null)
                throw cachedLastException;
            return null;
        }
    }
}
