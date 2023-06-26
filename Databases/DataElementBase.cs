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

    private DataElementBase() { }

    protected DataElementBase(MainDatabase databaseRef) => _databaseRef = databaseRef;

    #endregion
}