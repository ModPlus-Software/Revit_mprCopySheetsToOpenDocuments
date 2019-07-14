namespace mprCopySheetsToOpenDocuments
{
    using System;
    using System.Collections.Generic;
    using ModPlusAPI.Interfaces;

    public class ModPlusConnector : IModPlusFunctionInterface
    {
        private static ModPlusConnector _instance;

        public static ModPlusConnector Instance => _instance ?? (_instance = new ModPlusConnector());

        public SupportedProduct SupportedProduct => SupportedProduct.Revit;

        public string Name => "mprCopySheetsToOpenDocuments";

#if R2015
        public string AvailProductExternalVersion => "2015";
#elif R2016
        public string AvailProductExternalVersion => "2016";
#elif R2017
        public string AvailProductExternalVersion => "2017";
#elif R2018
        public string AvailProductExternalVersion => "2018";
#elif R2019
        public string AvailProductExternalVersion => "2019";
#elif R2020
        public string AvailProductExternalVersion => "2020";
#endif

        public string FullClassName => "mprCopySheetsToOpenDocuments.Command";

        public string AppFullClassName => string.Empty;

        public Guid AddInId => Guid.Empty;

        public string LName => "Копировать листы в открытые документы";

        public string Description => "Потом напишу =)";

        public string Author => "Похомов Максим";

        public string Price => "0";

        public bool CanAddToRibbon => true;

        public string FullDescription => "И это потом напишу =)";

        public string ToolTipHelpImage => string.Empty;

        public List<string> SubFunctionsNames => new List<string>();

        public List<string> SubFunctionsLames => new List<string>();

        public List<string> SubDescriptions => new List<string>();

        public List<string> SubFullDescriptions => new List<string>();

        public List<string> SubHelpImages => new List<string>();

        public List<string> SubClassNames => new List<string>();
    }
}
