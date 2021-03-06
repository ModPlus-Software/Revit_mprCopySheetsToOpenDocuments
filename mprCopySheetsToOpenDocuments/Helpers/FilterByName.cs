﻿namespace mprCopySheetsToOpenDocuments.Helpers
{
    using Autodesk.Revit.DB;

    /// <summary>
    /// Утилиты фильтрации элементов по содержимому параметров
    /// </summary>
    public class FilterByName
    {
        /// <summary>
        /// Фильтрует на четкое совпадение текста в заданном параметре, возвращает коллектор найденных элементов
        /// </summary>
        /// <param name="bip">BuiltInParameter</param>
        /// <param name="searchText">Искомый текст в параметре</param>
        /// <param name="doc">Класс документа для поиска</param>
        /// <returns></returns>
        public static FilteredElementCollector FilterElementByNameEqualsCollector(BuiltInParameter bip, string searchText, Document doc)
        {
            var nameParamId = new ElementId(bip);
            var pvp = new ParameterValueProvider(nameParamId);
            var evaluator = new FilterStringEquals();
#if R2017 || R2018 || R2019 || R2020 || R2021
            var rule = new FilterStringRule(pvp, evaluator, searchText, false);
#else
            var rule = new FilterStringRule(pvp, evaluator, searchText);
#endif
            var paramFilter = new ElementParameterFilter(rule);
            var selectElement = new FilteredElementCollector(doc).WherePasses(paramFilter);
            return selectElement;
        }

        /// <summary>
        /// Фильтрует по содержанию текста в заданном параметре, возвращает коллектор найденных элементов 
        /// </summary>
        /// <param name="bip">BuiltInParameter</param>
        /// <param name="searchText">Искомый текст в параметре</param>
        /// <param name="doc">Класс документа для поиска</param>
        /// <returns></returns>
        public static FilteredElementCollector FilterElementByNameContainsCollector(BuiltInParameter bip, string searchText, Document doc)
        {
            var nameParamId = new ElementId(bip);
            var pvp = new ParameterValueProvider(nameParamId);
            var evaluator = new FilterStringContains();
#if R2017 || R2018 || R2019 || R2020 || R2021
            var rule = new FilterStringRule(pvp, evaluator, searchText, false);
#else
            var rule = new FilterStringRule(pvp, evaluator, searchText);
#endif
            var paramFilter = new ElementParameterFilter(rule);
            var selectElement = new FilteredElementCollector(doc).WherePasses(paramFilter);
            return selectElement;
        }
    }
}
