using OOPsl.DocumentFunctions.Commands;
using OOPsl.UserFunctions;

namespace OOPsl.DocumentFunctions
{
    public class Document : ISubject
    {
        public string FileName { get; set; }
        public string Content { get; set; } = string.Empty;

        public List<string> VersionHistory { get; set; } = new List<string>();

        public CommandManager CommandManager { get; set; } = new CommandManager();

        private List<IObserver> observers = new List<IObserver>();
        public void Attach(IObserver observer) { observers.Add(observer); }
        public void Detach(IObserver observer) { observers.Remove(observer); }
        public void Notify() { 
            foreach (var obs in observers) obs.Update(this); 
        }
    }
}