using System.Linq.Expressions;
using Domain.Exceptions;

namespace Infrastructure.Utilities
{
    public static class ExpressionExtensions
    {
        public static string GetMemberName<T, TValue>(this Expression<Func<T, TValue>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
                return memberExpression.Member.Name;

            throw new InvalidArgumentException("Invalid expression");
        }
    }
}
