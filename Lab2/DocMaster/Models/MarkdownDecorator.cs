
namespace DocMaster.Models
{
    public class BoldDecorator : TextDecorator
    {
        public BoldDecorator(ITextComponent component) : base(component) { }

        public override string GetFormattedText() => $"**{base.GetFormattedText()}**";
    }

    public class ItalicDecorator : TextDecorator
    {
        public ItalicDecorator(ITextComponent component) : base(component) { }

        public override string GetFormattedText() => $"*{base.GetFormattedText()}*";
    }

    public class UnderlineDecorator : TextDecorator
    {
        public UnderlineDecorator(ITextComponent component) : base(component) { }

        public override string GetFormattedText() => $"~~{base.GetFormattedText()}~~";
    }
}
