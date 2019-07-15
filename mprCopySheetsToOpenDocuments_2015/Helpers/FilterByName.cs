namespace mprCopySheetsToOpenDocuments.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Autodesk.Revit.DB;

    public class FilterByName
    {
        /// <summary>
        /// Фильтрует на четкое совпадение текста в заданном параметре, возвращает коллектор найденных элементов
        /// </summary>
        /// <param name="bip">BuiltInParameter</param>
        /// <param name="searchText">Искомый текст в параметре</param>
        /// <param name="doc">Класс документа для поиска</param>
        /// <returns></returns>
        public static FilteredElementCollector FilterElementByNameEqualsCollector(BuiltInParameter bip, String searchText, Document doc)
        {
            ElementId nameParamId = new ElementId(bip);
            ParameterValueProvider pvp = new ParameterValueProvider(nameParamId);
            FilterStringEquals evaluator = new FilterStringEquals();
            FilterStringRule rule = new FilterStringRule(pvp, evaluator, searchText, false);
            ElementParameterFilter paramFilter = new ElementParameterFilter(rule);
            FilteredElementCollector selectElement = new FilteredElementCollector(doc).WherePasses(paramFilter);
            return selectElement;
        }

        /// <summary>
        /// Фильтрует по содержанию текста в заданном параметре, возвращает коллектор найденных элементов 
        /// </summary>
        /// <param name="bip">BuiltInParameter</param>
        /// <param name="searchText">Искомый текст в параметре</param>
        /// <param name="doc">Класс документа для поиска</param>
        /// <returns></returns>
        public static FilteredElementCollector FilterElementByNameContainsCollector(BuiltInParameter bip, String searchText, Document doc)
        {            
            ElementId nameParamId = new ElementId(bip);
            ParameterValueProvider pvp = new ParameterValueProvider(nameParamId);
            FilterStringContains evaluator = new FilterStringContains();
            FilterStringRule rule = new FilterStringRule(pvp, evaluator, searchText, false);
            ElementParameterFilter paramFilter = new ElementParameterFilter(rule);
            FilteredElementCollector selectElement = new FilteredElementCollector(doc).WherePasses(paramFilter);
            return selectElement;
        }
    }
}
