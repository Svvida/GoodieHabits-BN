using Domain.Enum;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.ModelBinders
{
    public class QuestTypeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext is null)
                throw new InvalidArgumentException(nameof(bindingContext));

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (Enum.TryParse<QuestTypeEnum>(value, true, out var questTypeEnum))
            {
                bindingContext.Result = ModelBindingResult.Success(questTypeEnum);
            }
            else
            {
                bindingContext.ModelState.AddModelError(
                    bindingContext.ModelName,
                    $"'{value}' is not a valid quest type. Valid types are: {string.Join(",", Enum.GetNames(typeof(QuestTypeEnum)))}");
            }

            return Task.CompletedTask;
        }
    }
}
