using MarioPizzaImport.Import;

namespace MarioPizzaImport.Command
{
    public class CommandImportIngredient: CommandImport<ingredient>
    {
        public override string GetCommandName()
        {
            return "import-ingredient";
        }

        protected override Importer<ingredient> CreateImporter(dbi298845_prangersEntities database, countrycode countrycode)
        {
            return new IngredientImporter(database, countrycode);
        }
    }
}