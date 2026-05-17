using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using ExpressionEvaluator.Commands;
using ExpressionEvaluator.Core.Evaluating;
using ExpressionEvaluator.Core.Lexing;
using ExpressionEvaluator.Core.Parsing;

namespace ExpressionEvaluator.ViewModels;

public sealed class EvaluatorViewModel : ViewModelBase
{
    private string _expressionText = "";
    private string _variablesText = "";
    private string _resultText = "";
    private string _errorText = "";

    public ObservableCollection<Token> Tokens { get; } = new();

    public ICommand EvaluateCommand { get; }

    public EvaluatorViewModel()
    {
        EvaluateCommand = new RelayCommand(
            execute: _ => Evaluate(),
            canExecute: _ => !string.IsNullOrWhiteSpace(ExpressionText)
        );
    }

    public string ExpressionText
    {
        get => _expressionText;
        set
        {
            if (SetField(ref _expressionText, value))
            {
                ClearOutputs();
                (EvaluateCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string VariablesText
    {
        get => _variablesText;
        set
        {
            if (SetField(ref _variablesText, value))
            {
                ClearOutputs();
            }
        }
    }

    public string ResultText
    {
        get => _resultText;
        private set => SetField(ref _resultText, value);
    }

    public string ErrorText
    {
        get => _errorText;
        private set => SetField(ref _errorText, value);
    }

    private void Evaluate()
    {
        ClearOutputs();

        IReadOnlyList<Token> tokens;
        try
        {
            tokens = new Lexer(ExpressionText).Tokenize();
        }
        catch (LexerException ex)
        {
            ErrorText = $"Lexer error: {ex.Message}";
            return;
        }

        foreach (var token in tokens)
        {
            Tokens.Add(token);
        }

        Core.Parsing.Ast.Expression ast;
        try
        {
            ast = new Parser(tokens).Parse();
        }
        catch (ParserException ex)
        {
            ErrorText = $"Parser error: {ex.Message}";
            return;
        }

        Variables variables;
        try
        {
            variables = ParseVariables(VariablesText);
        }
        catch (FormatException ex)
        {
            ErrorText = $"Variables error: {ex.Message}";
            return;
        }

        try
        {
            var value = new Evaluator().Evaluate(ast, variables);
            ResultText = FormatValue(value);
        }
        catch (EvaluatorException ex)
        {
            ErrorText = $"Evaluator error: {ex.Message} (position {ex.Position})";
        }
    }

    private void ClearOutputs()
    {
        Tokens.Clear();
        ResultText = "";
        ErrorText = "";
    }

    private static Variables ParseVariables(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Variables.Empty;
        }

        var dict = new Dictionary<string, Value>();
        var pairs = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                throw new FormatException($"Invalid variable assignment: '{pair}'. Expected 'name = value'.");
            }

            var name = parts[0];
            var rawValue = parts[1];

            if (rawValue.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                dict[name] = new BooleanValue(true);
            }
            else if (rawValue.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                dict[name] = new BooleanValue(false);
            }
            else if (double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                dict[name] = new NumberValue(number);
            }
            else
            {
                throw new FormatException($"Cannot parse value '{rawValue}' for variable '{name}'.");
            }
        }

        return new Variables(dict);
    }

    private static string FormatValue(Value value) => value switch
    {
        NumberValue n => n.Number.ToString(CultureInfo.InvariantCulture),
        BooleanValue b => b.Boolean ? "true" : "false",
        _ => value.ToString() ?? ""
    };
}
