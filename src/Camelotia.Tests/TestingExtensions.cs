using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace Camelotia.Tests
{
    public static class TestingExtensions
    {
        public static bool CanExecute(this IReactiveCommand command)
        {
            var canExecute = false;
            command.CanExecute.Take(1).Subscribe(value => canExecute = value);
            return canExecute;
        }
    }
}