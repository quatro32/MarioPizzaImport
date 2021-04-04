using MarioPizzaImport.Import;

namespace MarioPizzaImport.Command
{
    public class CommandImportOrder: CommandImport<order>
    {
        public override string GetCommandName()
        {
            return "import-order";
        }

        protected override Importer<order> CreateImporter(dbi298845_prangersEntities database, countrycode countrycode)
        {
            return new OrderImporter(database, countrycode);
        }
    }
}