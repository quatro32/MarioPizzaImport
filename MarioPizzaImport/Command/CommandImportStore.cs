using MarioPizzaImport.Import;

namespace MarioPizzaImport.Command
{
    public class CommandImportStore: CommandImport<store>
    {
        public override string GetCommandName()
        {
            return "import-store";
        }

        protected override Importer<store> CreateImporter(dbi298845_prangersEntities database, countrycode countrycode)
        {
            return new StoreImporter(database, countrycode);
        }
    }
}