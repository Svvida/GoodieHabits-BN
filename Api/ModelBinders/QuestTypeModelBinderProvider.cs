using Domain.Enum;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.ModelBinders
{
    public class QuestTypeModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context is null)
                throw new InvalidArgumentException(nameof(context));

            if (context.Metadata.ModelType == typeof(QuestTypeEnum))
                return new QuestTypeModelBinder();

            return null;
        }
    }
}
