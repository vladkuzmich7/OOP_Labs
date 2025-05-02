namespace OOPsl.DocumentFunctions.Commands
{
    public interface ICommand
    {
        void Execute();
        void UnExecute();
        string UpdatedText { get; }
    }
}
