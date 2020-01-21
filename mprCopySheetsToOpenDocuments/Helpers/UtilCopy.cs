namespace mprCopySheetsToOpenDocuments.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;

    /// <summary>
    /// Методы копирования видов
    /// </summary>
    public class UtilCopy
    {
        /// <summary>
        /// Копировать чертежный вид
        /// </summary>
        /// <param name="activeDocument">Активный документ</param>
        /// <param name="sheet">Лист</param>
        /// <param name="sheetNew">Новый лист</param>
        /// <param name="destinationDocument">Документ назначения</param>
        /// <param name="cpOptions">Опции копирования</param>
        public static void CopyDraftingView(
            Document activeDocument,
            ViewSheet sheet,
            ViewSheet sheetNew,
            Document destinationDocument,
            CopyPasteOptions cpOptions)
        {
            var viewPortsId = sheet.GetAllViewports();
            var viewContents = new List<ElementId>();
            var draftingViews = new FilteredElementCollector(destinationDocument).OfClass(typeof(ViewDrafting));
            if (viewPortsId.Any())
            {
                foreach (var viewPortId in viewPortsId)
                {
                    if (activeDocument.GetElement(viewPortId) is Viewport viewport)
                    {
                        var viewportItem = activeDocument.GetElement(viewport.ViewId);
                        if (viewportItem is ViewDrafting view && view.ViewType == ViewType.DraftingView)
                        {
                            var newView = ViewDrafting.Create(destinationDocument, destinationDocument.GetDefaultElementTypeId(ElementTypeGroup.ViewTypeDrafting));
                            newView.Scale = view.Scale;
                            newView.Name = GetSuffixNameElements(draftingViews.ToList(), view.Name);
                            var viewContentsDrafting = new FilteredElementCollector(activeDocument).OwnedByView(view.Id);
                            foreach (var item in viewContentsDrafting)
                            {
                                if (item.Category != null && item.Category.Id.IntegerValue != -2000055)
                                    viewContents.Add(item.Id);
                            }

                            if (viewContents.Any())
                            {
                                ElementTransformUtils.CopyElements(view, viewContents, newView, null, cpOptions);
                            }

                            destinationDocument.Regenerate();
                            var searchText = activeDocument.GetElement(viewport.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
                            var newViewPort = Viewport.Create(destinationDocument, sheetNew.Id, newView.Id, viewport.GetBoxCenter());
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
                                var nameType = viewport.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString();
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
                            var searchText = activeDocument.GetElement(viewport.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
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
        /// <param name="activeDocument">Активный документ</param>
        /// <param name="sheet">Лист</param>
        /// <param name="sheetNew">Новый лист</param>
        /// <param name="destinationDocument">Документ назначения</param>
        /// <param name="cpOptions">Опции копирования</param>
        public static void CopyLegend(Document activeDocument, ViewSheet sheet, ViewSheet sheetNew, Document destinationDocument, CopyPasteOptions cpOptions)
        {
            var viewPortsId = sheet.GetAllViewports();

            var destinationViewLegend = new FilteredElementCollector(destinationDocument)
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
                            var viewContents = new FilteredElementCollector(activeDocument)
                                .OwnedByView(viewLegend.Id)
                                .ToElementIds()
                                .Where(x => activeDocument.GetElement(x).Category != null && 
                                            activeDocument.GetElement(x).Category.Id.IntegerValue != -2000055)
                                .ToList();
                            if (viewContents.Any())
                            {
                                if (destinationDocument.GetElement(destinationViewLegend.First().Duplicate(ViewDuplicateOption.Duplicate)) is View newViewLegend)
                                {
                                    newViewLegend.Name = GetSuffixNameView(destinationViewLegend, viewLegend.Name);
                                    newViewLegend.Scale = viewLegend.Scale;
                                    ElementTransformUtils.CopyElements(viewLegend, viewContents, newViewLegend, null, cpOptions);
                                    var newViewPort = Viewport.Create(destinationDocument, sheetNew.Id, newViewLegend.Id, viewport.GetBoxCenter());

                                    destinationDocument.Regenerate();
                                    var searchText = activeDocument.GetElement(viewport.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
                                    if (CheckElementName(BuiltInParameter.ALL_MODEL_TYPE_NAME, searchText, destinationDocument))
                                    {
                                        var viewPortsTypeIds = ElementTransformUtils.CopyElements(activeDocument, new List<ElementId> { viewport.GetTypeId() }, destinationDocument, null, cpOptions);
                                        newViewPort.ChangeTypeId(viewPortsTypeIds.First());
                                    }
                                    else
                                    {
                                        var nameType = viewport.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString();
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
            }
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

        private static string GetSuffixNameView(IEnumerable<View> elements, string name)
        {
            var tempLis = elements.Select(item => item.Name).ToList();

            if (tempLis.Contains(name))
            {
                for (var index = 1; index < 999; index++)
                {
                    var nameNew = $"{name} Copy {index}";
                    if (!tempLis.Contains(nameNew))
                    {
                        return nameNew;
                    }
                }
            }

            return name;
        }

        /// <summary>
        /// Получает имя листа с совпавшим числовым суффиксом в номере
        /// </summary>
        /// <param name="elements">Коллекция элементов</param>
        /// <param name="name">Имя листа</param>
        public static string GetSuffixNumberViewSheet(List<Element> elements, string name)
        {
            var tempLis = elements
                .Select(item => item.get_Parameter(BuiltInParameter.SHEET_NUMBER)?.AsString() ?? string.Empty)
                .ToList();

            if (tempLis.Contains(name))
            {
                //// TaskDialog.Show("Нашел", string.Join("\n", tempLis));
                for (var index = 1; index < 999; index++)
                {
                    var nameNew = $"{name} Copy {index}";
                    if (!tempLis.Contains(nameNew))
                    {
                        return nameNew;
                    }
                }
            }

            return name;
        }

        /// <summary>
        /// Получает имя листа с совпавшим числовым суффиксом в имени
        /// </summary>
        /// <param name="elements">Коллекция элементов</param>
        /// <param name="name">Имя листа</param>
        public static string GetSuffixNameElements(List<Element> elements, string name)
        {
            var tempLis = elements.Select(item => item.Name).ToList();

            if (tempLis.Contains(name))
            {
                for (var index = 1; index < 999; index++)
                {
                    var nameNew = $"{name} Copy {index}";
                    if (!tempLis.Contains(nameNew))
                    {
                        return nameNew;
                    }
                }
            }

            return name;
        }
    }
}
