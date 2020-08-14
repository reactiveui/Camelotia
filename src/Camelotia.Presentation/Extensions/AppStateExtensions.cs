using System;
using System.Linq.Expressions;
using System.Reflection;
using ReactiveUI;

namespace Camelotia.Presentation.Extensions
{
    public static class AppStateExtensions
    {
        public static IDisposable AutoUpdate<TDestinationObject, TSourceObject, TProperty>(
            this TSourceObject source,
            Expression<Func<TSourceObject, TProperty>> sourceProperty,
            TDestinationObject destination,
            Expression<Func<TDestinationObject, TProperty>> destinationProperty,
            bool assignInitialValueToSourceFromDestination = true) 
            where TSourceObject : class
            where TDestinationObject : class
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (sourceProperty == null) throw new ArgumentNullException(nameof(sourceProperty));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (destinationProperty == null) throw new ArgumentNullException(nameof(destinationProperty));
            if (assignInitialValueToSourceFromDestination)
                SetProperty(source, sourceProperty, destinationProperty.Compile()(destination));
            
            return source
                .WhenAnyValue(sourceProperty)
                .Subscribe(value => SetProperty(destination, destinationProperty, value));

            void SetProperty<TObject>(TObject instance,
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