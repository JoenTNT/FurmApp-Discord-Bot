namespace FurmAppDBot.Databases;

/// <summary>
/// To load data on element itself.
/// </summary>
public interface ILoadDatabase
{
    /// <summary>
    /// Load data from database.
    /// </summary>
    Task<bool> LoadData();
}