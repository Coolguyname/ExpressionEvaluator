namespace ExpressionEvaluator.Core.Evaluating;

public abstract record Value
{
    public abstract string TypeName { get; }
}

public sealed record NumberValue(double Number) : Value
{
    public override string TypeName => "Number";
}

public sealed record BooleanValue(bool Boolean) : Value
{
    public override string TypeName => "Boolean";
}
