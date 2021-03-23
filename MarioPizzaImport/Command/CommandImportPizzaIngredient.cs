using MarioPizzaImport.Import;

namespace MarioPizzaImport.Command
{
    public class CommandImportPizzaIngredient: CommandImport<productingredient>
    {
        public override string GetCommandName()
        {
            return "import-pizza-ingredient";
        }

        protected override Importer<productingredient> CreateImporter(dbi298845_prangersEntities database, countrycode countrycode)
        {
            return new ProductIngredientImport(database, countrycode);
        }
    }
}