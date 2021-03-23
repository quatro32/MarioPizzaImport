using System;
using System.Collections.Generic;
using System.Linq;

namespace MarioPizzaImport.Command
{
    public class CommandRouter
    {
        private Dictionary<string, Command> _allCommand;
        private dbi298845_prangersEntities _database;
        private countrycode _countrycode;

        public CommandRouter(dbi298845_prangersEntities database, countrycode countrycode)
        {
            _allCommand = new Dictionary<string, Command>();
            _database = database;
            _countrycode = countrycode;
        }

        public void Register(Command command)
        {
            string commandName = command.GetCommandName().ToLower();

            if (_allCommand.ContainsKey(commandName))
            {
                throw new Exception(string.Format("Command '{0}' was already registered.", commandName));
            }

            _allCommand.Add(commandName, command);
        }

        public void Execute(string[] allInputString)
        {
            List<Tuple<string, Command>> allCommandToExecute = new List<Tuple<string, Command>>();
            
            foreach (string commandString in allInputString)
            {
                List<string> allCommandPart = commandString.Split(new []{' ', '='}, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (allCommandPart.Count <= 2)
                {
                    string commandName = allCommandPart[0];
                    string commandArgument = allCommandPart.Count == 2 ? allCommandPart[1] : "";

                    // Add the command with the path the the list that should be executed.
                    // That way we can first resolve the entire list of commands before starting the import operations.
                    allCommandToExecute.Add(
                        new Tuple<string, Command>(commandArgument, FindCommand(commandName))
                    );
                }
                else
                {
                    throw new Exception(string.Format("Input string '{0}' is invalid.", commandString));
                }
            }
            
            // Command parsing worked, now execute all commands.
            foreach (Tuple<string, Command> fileCommandTuple in allCommandToExecute)
            {
                Command command = fileCommandTuple.Item2;

                // If we're using an command with database connection, make sure to initialize it.
                CommandDatabase commandImporter = command as CommandDatabase;
                commandImporter?.Initialize(_database, _countrycode);

                command.Execute(fileCommandTuple.Item1);
            }

            if (allCommandToExecute.Count == 0)
            {
                PrintHelp();
            }
        }

        private Command FindCommand(string commandName)
        {
            if (_allCommand.ContainsKey(commandName.ToLower()))
            {
                return _allCommand[commandName.ToLower()];
            }

            throw new Exception(string.Format("Command '{0}' not found.", commandName));
        }

        private void PrintHelp()
        {
            Console.WriteLine("Usage of MarioPizzaImporter, is as follows: ");
            Console.WriteLine("    import.exe --option=\"value\" --option2=\"value2\"");
            Console.WriteLine("Possible options are:");

            foreach (KeyValuePair<string, Command> command in _allCommand)
            {
                Console.WriteLine("    --{0}=\"C:\\somefilepath\\somefile.csv\"", command.Key);
            }
        }
    }
}