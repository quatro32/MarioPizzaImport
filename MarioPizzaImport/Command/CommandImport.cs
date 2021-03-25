using MarioPizzaImport.Import;

namespace MarioPizzaImport.Command
{
    public abstract class CommandImport<T> : CommandDatabase
    {
        private Importer<T> _importer;

        public override void Execute(string input)
        {
            _importer.Run(input);
        }

        public override void Initialize(dbi298845_prangersEntities database, countrycode countrycode)
        {
            _importer = CreateImporter(database, countrycode);
        }

        protected abstract Importer<T> CreateImporter(dbi298845_prangersEntities database, countrycode countrycode);
    }
}