using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;

namespace Camelotia.Presentation.Avalonia.Services
{
    public sealed class AvaloniaStyleManager
    {
        public enum Theme
        {
            Citrus,
            Sea,
            Rust,
            Candy,
            Magma
        }

        private readonly StyleInclude _magmaStyle = CreateStyle("avares://Citrus.Avalonia/Magma.xaml");
        private readonly StyleInclude _candyStyle = CreateStyle("avares://Citrus.Avalonia/Candy.xaml");
        private readonly StyleInclude _citrusStyle = CreateStyle("avares://Citrus.Avalonia/Citrus.xaml");
        private readonly StyleInclude _rustStyle = CreateStyle("avares://Citrus.Avalonia/Rust.xaml");
        private readonly StyleInclude _seaStyle = CreateStyle("avares://Citrus.Avalonia/Sea.xaml");
        private readonly Window _window;

        public AvaloniaStyleManager(Window window)
        {
            _window = window;
            if (window.Styles.Count == 0)
                window.Styles.Add(_seaStyle);
            else window.Styles[0] = _seaStyle;
        }

        public Theme CurrentTheme { get; private set; } = Theme.Sea;

        public void UseTheme(Theme theme) =>
            _window.Styles[0] = (CurrentTheme = theme) switch
            {
                Theme.Citrus => _citrusStyle,
                Theme.Sea => _seaStyle,
                Theme.Rust => _rustStyle,
                Theme.Candy => _candyStyle,
                Theme.Magma => _magmaStyle,
                _ => throw new ArgumentOutOfRangeException(nameof(theme))
            };

        public void UseNextTheme() =>
            UseTheme(CurrentTheme switch
            {
                Theme.Citrus => Theme.Sea,
                Theme.Sea => Theme.Rust,
                Theme.Rust => Theme.Candy,
                Theme.Candy => Theme.Magma,
                Theme.Magma => Theme.Citrus,
                _ => throw new ArgumentOutOfRangeException(nameof(CurrentTheme))
            });

        private static StyleInclude CreateStyle(string url)
        {
            var self = new Uri("resm:Styles?assembly=Citrus.Avalonia.Sandbox");
            return new StyleInclude(self)
            {
                Source = new Uri(url)
            };
        }
    }
}
