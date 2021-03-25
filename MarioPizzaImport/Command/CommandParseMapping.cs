namespace MarioPizzaImport.Command
{
    public class CommandParseMapping: CommandDatabase
    {
        private dbi298845_prangersEntities _database;
        
        public override string GetCommandName()
        {
            return "parse-mapping";
        }

        public override void Execute(string input)
        {
            MappingParser mappingParser = new MappingParser(_database);
            mappingParser.ParseMappingFromOrderFile(input);
        }

        public override void Initialize(dbi298845_prangersEntities database, countrycode countrycode)
        {
            _database = database;
        }
    }
}