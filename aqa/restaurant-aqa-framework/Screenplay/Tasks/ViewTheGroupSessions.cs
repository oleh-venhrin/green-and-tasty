using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Auth;
using Restaurant_Aqa_Framework.Api.Dtos.GroupOrders;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class ViewTheGroupSessions(string? groupId = null, string? bearerToken = null) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var grpId = groupId ?? actor.Recall<GuestInviteResponse>().GroupReservationId;
        var token = bearerToken;
        if (token is null && actor.TryRecall<SignInResponse>(out var signin))
            token = signin.IdToken;

        var api = new GroupOrdersApi(actor.UsingTheApi());
        await actor.TheLastResponse().CaptureAsync(
            await api.GetGroupSessionsAsync(grpId, token));
    }
}
