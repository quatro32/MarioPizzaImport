using MarioPizzaImport.Import;

namespace MarioPizzaImport.Command
{
    public class CommandImportPostalCode: CommandImport<postalcode>
    {
        public override string GetCommandName()
        {
            return "import-postalcode";
        }

        protected override Importer<postalcode> CreateImporter(dbi298845_prangersEntities database, countrycode countrycode)
        {
            return new PostalCodeImporter(database, countrycode);
        }
    }
}