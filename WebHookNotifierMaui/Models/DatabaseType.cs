namespace WebHookNotifierMaui.Models
{
    /// <summary>
    /// Defines the type of database to use for notification history
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// Use SQLite database (local file)
        /// </summary>
        SQLite,
        
        /// <summary>
        /// Use SQL Server database (remote)
        /// </summary>
        SQLServer
    }
}
