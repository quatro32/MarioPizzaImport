namespace MarioPizzaImport.Command
{
    public abstract class CommandDatabase: Command
    {
        public abstract void Initialize(dbi298845_prangersEntities database, countrycode countrycode);
    }
}