namespace mprCopySheetsToOpenDocuments.Models
{
    /// <summary>
    /// Элемент дерева
    /// </summary>
    public interface IBrowserItem
    {
        /// <summary>
        /// Элемент отмечен
        /// </summary>
        bool Checked { get; set; }
    }
}
