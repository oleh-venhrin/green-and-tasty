using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Auth;
using Restaurant_Aqa_Framework.Api.Dtos.GroupOrders;
using Restaurant_Aqa_Framework.Screenplay.Abilities;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class RemoveGuestFromGroup(
    string guestName,
    string? groupId = null,
    string? bearerToken = null) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var grpId = groupId ?? actor.Recall<GuestInviteResponse>().GroupReservationId;

        if (actor.Can<IBrowseTheWeb>())
        {
            var page = new GroupOrderPage(actor.LookingAtThePage());
            var card = page.ParticipantByName(guestName);
            await card.RemoveAsync();
            return;
        }

        var session = actor.Recall<GuestSessionResponse>(guestName);
        var token = bearerToken;
        if (token is null && actor.TryRecall<SignInResponse>(out var signin))
            token = signin.IdToken;

        var api = new GroupOrdersApi(actor.UsingTheApi());
        await actor.TheLastResponse().CaptureAsync(
            await api.RemoveGuestSessionAsync(grpId, session.SessionId, token));
    }
}

