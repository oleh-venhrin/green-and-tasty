using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Auth;
using Restaurant_Aqa_Framework.Api.Dtos.GroupOrders;
using Restaurant_Aqa_Framework.Api.Dtos.Reservations;
using Restaurant_Aqa_Framework.Screenplay.Abilities;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class SubmitTheGroupPreOrder(
    string? groupId = null,
    string? reservationId = null,
    string? bearerToken = null) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var resId = reservationId ?? actor.Recall<ReservationResponse>().Id;
        var grpId = groupId ?? actor.Recall<GuestInviteResponse>().GroupReservationId;

        if (actor.Can<IBrowseTheWeb>())
        {
            var page = new GroupOrderPage(actor.LookingAtThePage());
            await page.SubmitPreOrderAsync();
            return;
        }

        var token = bearerToken;
        if (token is null && actor.TryRecall<SignInResponse>(out var signin))
            token = signin.IdToken;

        var api = new GroupOrdersApi(actor.UsingTheApi());
        await actor.TheLastResponse().CaptureAsync(
            await api.SubmitGroupPreOrderAsync(grpId, resId, token));
    }
}
