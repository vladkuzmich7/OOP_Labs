
namespace DocMaster.Command
{
    public interface ICommand
    {
        void Execute();
        void Undo();
        int CursorPositionBefore { get; }
        int CursorPositionAfter { get; }
    }
}
