using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ExpressionEvaluator.Commands;
using ExpressionEvaluator.Core.Lexing;
using System.Text;

namespace ExpressionEvaluator.ViewModels;

public sealed class TokenizerViewModel : ViewModelBase
{
    private string _inputText = "";
    private string _errorMessage = "";

    public string InputText { get { return _inputText; } set { if (SetField(ref _inputText, value)) { Tokens.Clear(); ErrorMessage = ""; } } }

    public string ErrorMessage { get { return _errorMessage; } set { SetField(ref _errorMessage, value); } }

    public ObservableCollection<Token> Tokens { get; } = new();

    public ICommand TokenizeCommand { get; }

    public TokenizerViewModel() 
    {
        TokenizeCommand = new RelayCommand(
            execute: Tokenize,
            canExecute: () => !string.IsNullOrWhiteSpace(InputText)
        );
    }

    private void Tokenize()
    {
        Tokens.Clear();
        ErrorMessage = "";

        try
        {
            var text = new Lexer(InputText).Tokenize();

            foreach (var token in text)
            {
                Tokens.Add(token);
            }
        }
        catch (LexerException ex) { ErrorMessage = ex.Message; }

    }
}

