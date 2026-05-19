using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ExpressionEvaluator.Commands;
using ExpressionEvaluator.Core.Evaluating;
using ExpressionEvaluator.Core.Lexing;
using ExpressionEvaluator.Core.Parsing;
using ExpressionEvaluator.Core.Parsing.Ast;

namespace ExpressionEvaluator.ViewModels;

public sealed class EvaluatorViewModel : ViewModelBase
{
    private string _expressionText = "";
    private string _variablesText = "";
    private string _resultText = "";
    private string _errorText = "";

    public ObservableCollection<Token> Tokens { get; } = [];

    public ObservableCollection<AstNodeViewModel> AstNodes { get; } = [];

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

        Expression ast;
        try
        {
            ast = new Parser(tokens).Parse();
        }
        catch (ParserException ex)
        {
            ErrorText = $"Parser error: {ex.Message} (position {ex.Position})";
            return;
        }

        AstNodes.Clear();
        AstNodes.Add(AstNodeViewModel.FromExpression(ast));

        Variables variables;
        try
        {
            variables = VariablesParser.Parse(VariablesText);
        }
        catch (VariablesParseException ex)
        {
            ErrorText = $"Variables error: {ex.Message}";
            return;
        }

        try
        {
            var value = new Evaluator().Evaluate(ast, variables);
            ResultText = ValueFormatter.Format(value);
        }
        catch (EvaluatorException ex)
        {
            ErrorText = $"Evaluator error: {ex.Message} (position {ex.Position})";
        }
    }

    private void ClearOutputs()
    {
        Tokens.Clear();
        AstNodes.Clear();
        ResultText = "";
        ErrorText = "";
    }
}
