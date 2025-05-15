
using DocMaster.Models;

public abstract class TextDecorator : ITextComponent
{
    protected ITextComponent _component;

    protected TextDecorator(ITextComponent component)
    {
        _component = component;
    }

    public virtual string GetFormattedText() => _component.GetFormattedText();
}

