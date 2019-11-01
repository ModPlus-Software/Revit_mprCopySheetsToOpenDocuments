using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace mprCopySheetsToOpenDocuments.Helpers
{
    public class UtilCopy
    {
        public static void copy_draftingview(Document activeDocument, ViewSheet sheet, ViewSheet sheetNew, Document destinationDocument, CopyPasteOptions cp_options)
        {
            var viewPortsId = sheet.GetAllViewports();
            var viewContents = new List<ElementId>();
            ViewDrafting new_view = null;
            ViewDrafting view = null;
            var viewDraftings = new FilteredElementCollector(destinationDocument).OfClass(typeof(ViewDrafting));
            if (viewPortsId.Any())
            {
                foreach (var viewPortId in viewPortsId)
                {
                    Viewport viewport = activeDocument.GetElement(viewPortId) as Viewport;
                    var viewportItem = activeDocument.GetElement(viewport.ViewId);
                    if (viewportItem is ViewDrafting && (viewportItem as ViewDrafting).ViewType == ViewType.DraftingView)
                    {
                        new_view = ViewDrafting.Create(destinationDocument, destinationDocument.GetDefaultElementTypeId(ElementTypeGroup.ViewTypeDrafting));
                        view = viewportItem as ViewDrafting;
                        new_view.Scale = view.Scale;
                        new_view.Name = GetSuffixNameElements(viewDraftings.ToList(), view.Name);
                        var viewContentsDrafting = new FilteredElementCollector(activeDocument).OwnedByView(viewportItem.Id);
                        foreach (var item in viewContentsDrafting)
                        {
                            if (item.Category != null && item.Category.Id.IntegerValue != -2000055)
                                viewContents.Add(item.Id);
                        }
                        if (viewContents.Any())
                        {
                            ElementTransformUtils.CopyElements(view, viewContents, new_view, null, cp_options);
                        }
                        destinationDocument.Regenerate();
                        string searchText = activeDocument.GetElement(viewport.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
                        var newViewPort = Viewport.Create(destinationDocument, sheetNew.Id, new_view.Id, viewport.GetBoxCenter());
                        if (сheckNameElement(BuiltInParameter.ALL_MODEL_TYPE_NAME, searchText, destinationDocument))
                        {
                            var viewPortsTypeIds = ElementTransformUtils.CopyElements(activeDocument, new List<ElementId>() { viewport.GetTypeId() }, destinationDocument, null, cp_options);
                            newViewPort.ChangeTypeId(viewPortsTypeIds.First());
                        }
                        else
                        {
                            string nameType = viewport.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString();
                            var viewPortType = Helpers.FilterByName.FilterElementByNameEqualsCollector(BuiltInParameter.ALL_MODEL_TYPE_NAME, nameType, destinationDocument).OfClass(typeof(ElementType)).First().Id;
                            newViewPort.ChangeTypeId(viewPortType);
                        }
                    }
                }
            }
        }

        public static void copy_ImageView(Document activeDocument, ViewSheet sheet, ViewSheet sheetNew, Document destinationDocument, CopyPasteOptions cp_options)
        {
            var viewPortsId = sheet.GetAllViewports();

            if (viewPortsId.Any())
            {
                foreach (var viewPortId in viewPortsId)
                {
                    Viewport viewport = activeDocument.GetElement(viewPortId) as Viewport;
                    var viewportItem = activeDocument.GetElement(viewport.ViewId);
                    if (viewportItem is ImageView && (viewportItem as ImageView).ViewType == ViewType.Rendering)
                    {
                        var ImageViewId = ElementTransformUtils.CopyElements(activeDocument, new List<ElementId>() { viewportItem.Id }, destinationDocument, null, cp_options);
                        destinationDocument.Regenerate();
                        string searchText = activeDocument.GetElement(viewport.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
                        var newViewPort = Viewport.Create(destinationDocument, sheetNew.Id, ImageViewId.First(), viewport.GetBoxCenter());
                        if (сheckNameElement(BuiltInParameter.ALL_MODEL_TYPE_NAME, searchText, destinationDocument))
                        {
                            var viewPortsTypeIds = ElementTransformUtils.CopyElements(activeDocument, new List<ElementId>() { viewport.GetTypeId() }, destinationDocument, null, cp_options);
                            newViewPort.ChangeTypeId(viewPortsTypeIds.First());
                        }
                        else
                        {
                            string nameType = viewport.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString();
                            var viewPortType = Helpers.FilterByName.FilterElementByNameEqualsCollector(BuiltInParameter.ALL_MODEL_TYPE_NAME, nameType, destinationDocument).OfClass(typeof(ElementType)).First().Id;
                            newViewPort.ChangeTypeId(viewPortType);
                        }
                    }
                }

            }
        }

        public static void copy_Legend(Document activeDocument, ViewSheet sheet, ViewSheet sheetNew, Document destinationDocument, CopyPasteOptions cp_options)
        {
            var viewPortsId = sheet.GetAllViewports();

            var destinationViewLegend = new FilteredElementCollector(destinationDocument).OfClass(typeof(View)).Cast<View>().Where(x => x.ViewType == ViewType.Legend);

            if (viewPortsId.Any())
            {
                foreach (var viewPortId in viewPortsId)
                {
                    Viewport viewport = activeDocument.GetElement(viewPortId) as Viewport;
                    var viewportItem = activeDocument.GetElement(viewport.ViewId);
                    if (viewportItem is View && (viewportItem as View).ViewType == ViewType.Legend)
                    {
                        View viewLegend = viewportItem as View;
                        var viewContents = new FilteredElementCollector(activeDocument).OwnedByView(viewLegend.Id).ToElementIds().Where(x => activeDocument.GetElement(x).Category != null && activeDocument.GetElement(x).Category.Id.IntegerValue != -2000055).ToList();
                        if (viewContents.Any())
                        {
                            
                            View newViewLegend = destinationDocument.GetElement(destinationViewLegend.First().Duplicate(ViewDuplicateOption.Duplicate)) as View;
                            newViewLegend.Name = GetSuffixNameView(destinationViewLegend, viewLegend.Name);
                            newViewLegend.Scale = viewLegend.Scale;
                            ElementTransformUtils.CopyElements(viewLegend, viewContents, newViewLegend, null, cp_options);
                            var newViewPort = Viewport.Create(destinationDocument, sheetNew.Id, newViewLegend.Id, viewport.GetBoxCenter());

                            destinationDocument.Regenerate();
                            string searchText = activeDocument.GetElement(viewport.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
                            if (сheckNameElement(BuiltInParameter.ALL_MODEL_TYPE_NAME, searchText, destinationDocument))
                            {
                                var viewPortsTypeIds = ElementTransformUtils.CopyElements(activeDocument, new List<ElementId>() { viewport.GetTypeId() }, destinationDocument, null, cp_options);
                                newViewPort.ChangeTypeId(viewPortsTypeIds.First());
                            }
                            else
                            {
                                string nameType = viewport.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString();
                                var viewPortType = Helpers.FilterByName.FilterElementByNameEqualsCollector(BuiltInParameter.ALL_MODEL_TYPE_NAME, nameType, destinationDocument).OfClass(typeof(ElementType)).First().Id;
                                newViewPort.ChangeTypeId(viewPortType);
                            }
                        }
                    }
                }
            }
        }

        public static bool copy_guideGrids(Document activeDocument, ViewSheet sheet, ViewSheet sheetNew, Document destinationDocument, CopyPasteOptions cp_options)
        {
            ElementId GuideGridsId = sheet.get_Parameter(BuiltInParameter.SHEET_GUIDE_GRID).AsElementId();
            if (GuideGridsId != ElementId.InvalidElementId)
            {
                List<ElementId> sheetGuideGridsId = new List<ElementId>();
                sheetGuideGridsId.Add(GuideGridsId);
                ElementTransformUtils.CopyElements(activeDocument, sheetGuideGridsId, destinationDocument, null, cp_options);
                var guide_elements = new FilteredElementCollector(destinationDocument).OfCategory(BuiltInCategory.OST_GuideGrid).WhereElementIsNotElementType().Where(x => x.Name == activeDocument.GetElement(GuideGridsId).Name);
                if (guide_elements.Any())
                {
                    sheetNew.get_Parameter(BuiltInParameter.SHEET_GUIDE_GRID).Set(guide_elements.First().Id);
                }
            }

            return true;
        }

        public static void copy_sheetRevisions(Document activeDocument, ViewSheet sheet, ViewSheet sheetNew, Document destinationDocument)
        {
            var revisions = Revision.GetAllRevisionIds(destinationDocument).Select(item => destinationDocument.GetElement(item) as Revision);//new FilteredElementCollector(destinationDocument).OfClass(typeof(Revision));            
            var sheetRevisions = sheet.GetAllRevisionIds().Select(x => activeDocument.GetElement(x) as Revision);
            ICollection<ElementId> sheetRevisionsNew = new List<ElementId>();
            foreach (Revision revision in sheetRevisions)
            {
                if (!revisions.Where(item => item.Description == revision.Description).Any())
                {
                    Revision newRevision = Revision.Create(destinationDocument);
                    newRevision.Description = revision.Description;
                    newRevision.IssuedBy = revision.IssuedBy;
                    newRevision.IssuedTo = revision.IssuedTo;
                    newRevision.NumberType = revision.NumberType;
                    newRevision.RevisionDate = revision.RevisionDate;
                    sheetRevisionsNew.Add(newRevision.Id);
                }
                else
                {
                    Revision desRevision = revisions.FirstOrDefault(item => item.Description == revision.Description);
                    sheetRevisionsNew.Add(desRevision.Id);
                }
            }
            sheetNew.SetAdditionalRevisionIds(sheetRevisionsNew);
        }

        public static bool сheckNameElement(BuiltInParameter bip, String searchText, Document destinationDocument)
        {
            var collectore = Helpers.FilterByName.FilterElementByNameEqualsCollector(bip, searchText, destinationDocument).FirstOrDefault(x => x.Name == searchText);
            if (collectore == null)
            {
                return true;
            }
            return false;
        }

        private static string GetSuffixNameView(IEnumerable<View> elements, string name)
        {
            var tempLis = elements.Select(item => item.Name);
           
            if (tempLis.Contains(name))
            {
                for (int index = 1; index < 999; index++)
                {
                    string nameNew = name + " Copy " + index.ToString();
                    if (!tempLis.Contains(nameNew))
                    {
                        return nameNew;
                    }
                }
            }
            return name;           
        }

        public static string GetSuffixNumberViewSheet(List<Element> elements, string name)
        {
            var tempLis = elements.Select(item => item.get_Parameter(BuiltInParameter.SHEET_NUMBER)?.AsString() ?? string.Empty);
            
            if (tempLis.Contains(name))
            {
                TaskDialog.Show("Нашел", string.Join("\n", tempLis));
                for (int index = 1; index < 999; index++)
                {
                    string nameNew = name + " Copy " + index.ToString();
                    if (!tempLis.Contains(nameNew))
                    {
                        return nameNew;
                    }
                }
            }
            return name;
        }

        public static string GetSuffixNameElements(List<Element> elements, string name)
        {
            var tempLis = elements.Select(item => item.Name);

            if (tempLis.Contains(name))
            {
                for (int index = 1; index < 999; index++)
                {
                    string nameNew = name + " Copy " + index.ToString();
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
