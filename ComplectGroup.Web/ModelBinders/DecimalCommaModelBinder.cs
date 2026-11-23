using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ComplectGroup.Web.ModelBinders;

/// <summary>
/// ModelBinder, который принимает десятичные числа с разделителем
/// в виде точки или запятой и приводит их к типу decimal.
/// </summary>
public class DecimalCommaModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
            throw new ArgumentNullException(nameof(bindingContext));

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var rawValue = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(rawValue))
            return Task.CompletedTask;

        // Нормализуем разделитель: и точка, и запятая -> точка (инвариантная культура)
        var normalized = rawValue.Trim().Replace(',', '.');

        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                "Некорректное число. Используйте точку или запятую как разделитель.");
        }

        return Task.CompletedTask;
    }
}
