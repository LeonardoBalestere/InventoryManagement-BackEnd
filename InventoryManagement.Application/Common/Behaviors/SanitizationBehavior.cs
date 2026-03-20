using Ganss.Xss;
using MediatR;
using System.Reflection;

namespace InventoryManagement.Application.Common.Behaviors;

public class SanitizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var sanitizer = new HtmlSanitizer();
        
        var stringProperties = request.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.CanWrite);

        foreach (var prop in stringProperties)
        {
            var value = prop.GetValue(request) as string;
            if (!string.IsNullOrEmpty(value))
            {
                var sanitized = sanitizer.Sanitize(value);
                // Sanitize will return empty if it stripped everything
                prop.SetValue(request, sanitized);
            }
        }

        return await next();
    }
}
