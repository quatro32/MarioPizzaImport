using MarioPizzaImport.Import;

namespace MarioPizzaImport.Command
{
    public class CommandImportBottom: CommandImport<bottom>
    {
        public override string GetCommandName()
        {
            return "import-bottom";
        }

        protected override Importer<bottom> CreateImporter(dbi298845_prangersEntities database, countrycode countrycode)
        {
            return new BottomImporter(database, countrycode);
        }
    }
}