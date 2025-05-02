namespace OOPsl.DocumentFunctions.Displays
{
    class PlainTextDisplayStrategy : IDisplayStrategy
    {
        public void Display(Document document)
        {
            Console.WriteLine(document.Content);
        }
    }
}