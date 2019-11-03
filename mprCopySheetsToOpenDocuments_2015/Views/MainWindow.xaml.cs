namespace mprCopySheetsToOpenDocuments.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Главное окно плагина
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Title = ModPlusAPI.Language.GetFunctionLocalName(ModPlusConnector.Instance.Name, ModPlusConnector.Instance.LName);
        }

        // https://stackoverflow.com/a/9494484/4944499
        private void TreeViewSelectedItem_OnHandler(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.BringIntoView();
                e.Handled = true;  
            }
        }
    }
}
