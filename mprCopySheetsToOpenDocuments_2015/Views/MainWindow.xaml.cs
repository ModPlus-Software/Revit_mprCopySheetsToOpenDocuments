namespace mprCopySheetsToOpenDocuments.Views
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // todo set title
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
