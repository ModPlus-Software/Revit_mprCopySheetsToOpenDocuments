namespace mprCopySheetsToOpenDocuments
{
    using System;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using ViewModels;
    using Views;

    /// <summary>
    /// Команда Revit
    /// </summary>
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        /// <inheritdoc />
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Statistic.SendCommandStarting(ModPlusConnector.Instance);

                if (commandData.Application.Application.Documents.Size < 2)
                {
                    // Необходимо открыть не менее двух документов
                    MessageBox.Show(Language.GetItem(ModPlusConnector.Instance.Name, "m1"), MessageBoxIcon.Close);
                    return Result.Cancelled;
                }

                var mainWindow = new MainWindow();
                var viewModel = new MainViewModel(commandData.Application, mainWindow);
                mainWindow.DataContext = viewModel;
                mainWindow.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
                return Result.Failed;
            }
        }
    }
}
