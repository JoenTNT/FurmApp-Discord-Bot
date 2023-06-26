namespace FurmAppDBot.Databases;

/// <summary>
/// To save the data into database itself.
/// </summary>
public interface ISaveDatabase
{
    /// <summary>
    /// Save data to database.
    /// </summary>
    Task<bool> SaveData();
}