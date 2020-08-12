using System;
using System.Linq.Expressions;
using System.Reflection;
using ReactiveUI;

namespace Camelotia.Presentation.Extensions
{
    public static class AppStateExtensions
    {
        public static IDisposable AutoUpdate<TAppState, TReactiveObject, TProperty>(
            this TReactiveObject viewModel,
            TAppState appState,
            Expression<Func<TReactiveObject, TProperty>> viewModelProperty,
            Expression<Func<TAppState, TProperty>> appStateProperty,
            bool useInitialStatePropertyValue = true) 
            where TReactiveObject : class, IReactiveObject
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            if (viewModelProperty == null) throw new ArgumentNullException(nameof(viewModelProperty));
            if (appState == null) throw new ArgumentNullException(nameof(appState));
            if (appStateProperty == null) throw new ArgumentNullException(nameof(appStateProperty));
            
            if (useInitialStatePropertyValue)
            {
                var propertyValue = appStateProperty.Compile()(appState);
                if (propertyValue != null)
                    SetProperty(viewModel, viewModelProperty, propertyValue);
            }
            
            return viewModel
                .WhenAnyValue(viewModelProperty)
                .Subscribe(value => SetProperty(appState, appStateProperty, value));

            void SetProperty<TObject>(
                TObject instance,
                Expression<Func<TObject, TProperty>> property, 
                TProperty value)
            {
                var propertyName = property.Body.GetMemberInfo().Name;
                var assignable = typeof(TObject).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (assignable == null)
                    throw new MemberAccessException($"Unable to access property named {propertyName}");
                assignable.SetValue(instance, value, null);
            }
        }
    }
}