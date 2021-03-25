namespace MarioPizzaImport.Command
{
    public abstract class Command
    {
        public abstract string GetCommandName();
        
        public abstract void Execute(string input);
    }
}