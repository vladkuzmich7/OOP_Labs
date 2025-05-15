using DocMaster.Data.Adapters;
using DocMaster.Roles.Observers;

namespace DocMaster.Models
{
    public class Document : ITextComponent
    {
        public string Name { get; set; }
        public DocumentFormat Format { get; set; }
        public string Content { get; set; }

        private List<IDocumentChangeObserver> _observers = new List<IDocumentChangeObserver>();

        public Document(string name, DocumentFormat format) 
        {
            Name = name;
            Format = format;
            Content = string.Empty;
        }
        public string ToJson() => new JsonAdapter().Convert(this);
        public string ToXml() => new XmlAdapter().Convert(this);
        public string ConvertTo(DocumentFormat format)
        {
            IDocumentAdapter adapter = format switch
            {
                DocumentFormat.TXT => new TxtAdapter(),
                DocumentFormat.JSON => new JsonAdapter(),
                DocumentFormat.XML => new XmlAdapter(),
                _ => throw new NotSupportedException($"Format {format} not supported")
            };

            return adapter.Convert(this);
        }
        public void Subscribe(IDocumentChangeObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Unsubscribe(IDocumentChangeObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyChange(string editedBy)
        {
            foreach (var observer in _observers)
            {
                observer.OnDocumentChanged(this, editedBy);
            }
        }
        public string GetFormattedText() => Content;
    }
}
