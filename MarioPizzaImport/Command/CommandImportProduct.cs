using MarioPizzaImport.Import;

namespace MarioPizzaImport.Command
{
    public class CommandImportProduct: CommandImport<product>
    {
        public override string GetCommandName()
        {
            return "import-product";
        }

        protected override Importer<product> CreateImporter(dbi298845_prangersEntities database, countrycode countrycode)
        {
            return new ProductImporter(database, countrycode);
        }
    }
}