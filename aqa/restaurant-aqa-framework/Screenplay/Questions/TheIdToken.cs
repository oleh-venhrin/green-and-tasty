using Restaurant_Aqa_Framework.Api.Dtos.Auth;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>The id token the last sign-in handed out, or null if that sign-in did not succeed.</summary>
public sealed class TheIdToken : IQuestion<string?>
{
    public Task<string?> AnsweredByAsync(Actor actor)
    {
        if (actor.TryRecall<SignInResponse>(out var signin))
            return Task.FromResult<string?>(signin.IdToken);

        var response = actor.TheLastResponse();
        if (response.Succeeded)
        {
            try
            {
                return Task.FromResult<string?>(response.Json<SignInResponse>().IdToken);
            }
            catch
            {
                return Task.FromResult<string?>(null);
            }
        }

        return Task.FromResult<string?>(null);
    }
}
