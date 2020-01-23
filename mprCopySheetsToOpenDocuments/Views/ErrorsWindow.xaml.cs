namespace mprCopySheetsToOpenDocuments.Views
{
    public partial class ErrorsWindow
    {
        public ErrorsWindow()
        {
            InitializeComponent();

            Title = ModPlusAPI.Language.GetItem(ModPlusConnector.Instance.Name, "h5");
        }
    }
}
