using System;
using System.Threading.Tasks;

namespace authenticationlab
{
    public class AwfulQueryStringAuthenticationEvents
    {
        public Func<ValidateCredentialsContext, Task> OnValidateUsername { get; set; } = context => Task.CompletedTask;

        public virtual Task ValidateUsername(ValidateCredentialsContext context) => OnValidateUsername(context);
    }
}
