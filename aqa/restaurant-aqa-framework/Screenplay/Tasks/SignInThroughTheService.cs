using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Auth;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Signs in with the sign-in service directly (no browser). The outcome is the scenario's last response.</summary>
public sealed class SignInThroughTheService(string email, string password) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var response = await new AuthApi(actor.UsingTheApi()).SignInAsync(new SignInRequest(email, password));
        var captured = actor.TheLastResponse();
        await captured.CaptureAsync(response);
        if (captured.Succeeded)
        {
            var auth = captured.Json<SignInResponse>();
            actor.Remember(auth);
        }
    }
}
