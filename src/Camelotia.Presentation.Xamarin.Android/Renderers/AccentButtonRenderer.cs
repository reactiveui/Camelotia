using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Camelotia.Presentation.Xamarin.Controls;
using Camelotia.Presentation.Xamarin.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Xamarin.Forms.Color;
using VisualElement = Xamarin.Forms.VisualElement;

[assembly: ExportRenderer(typeof(AccentButton), typeof(AccentButtonRenderer))]

namespace Camelotia.Presentation.Xamarin.Droid.Renderers
{
    public class AccentButtonRenderer : ButtonRenderer
    {
        public AccentButtonRenderer(Context context)
            : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement is AccentButton control)
                ApplyColors(control);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);
            if (args.PropertyName == nameof(Button.IsEnabled))
                ApplyColors((AccentButton)sender);
        }

        private static Color GetBackgroundColor(VisualElement control)
        {
            return control.IsEnabled
                ? Color.FromRgb(100, 83, 179)
                : Color.FromRgb(73, 53, 165);
        }

        private static Color GetForegroundColor(VisualElement control)
        {
            return control.IsEnabled
                ? Color.FromRgb(254, 254, 254)
                : Color.FromRgb(140, 123, 219);
        }

        private void ApplyColors(VisualElement control)
        {
            var background = GetBackgroundColor(control).ToAndroid();
            Control.Background.SetColorFilter(background, PorterDuff.Mode.Src);

            var foreground = GetForegroundColor(control).ToAndroid();
            Control.SetTextColor(foreground);
        }
    }
}
