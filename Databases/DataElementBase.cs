using FurmAppDBot.Databases.Exceptions;

namespace FurmAppDBot.Databases;

public abstract class DataElementBase
{
    #region Variables

    private readonly MainDatabase _databaseRef;

    #endregion

    #region Properties

    protected MainDatabase DatabaseRef => _databaseRef;

    #endregion

    #region Constructor

    protected DataElementBase(MainDatabase databaseRef) => _databaseRef = databaseRef;

    #endregion

    #region Main

    /// <summary>
    /// Connect between 2 data element with base.
    /// </summary>
    /// <param name="with">Element that will be connected with this element</param>
    /// <param name="keys">Search for keys, each element has different structure</param>
    /// <exception cref="ConnectElementFailedException">
    /// Some element cannot be connected one another.
    /// </exception>
    public abstract Task ConnectElement(DataElementBase with, params string[] keys);

    #endregion
}