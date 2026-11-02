using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.GroupOrders;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class JoinGroupAsGuest(
    string guestName,
    string dishId = "dish-01",
    int quantity = 1,
    bool submitCart = true) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var invite = actor.Recall<GuestInviteResponse>();
        var api = new GroupOrdersApi(actor.UsingTheApi());

        var createResp = await api.CreateGuestSessionAsync(invite.GroupReservationId, guestName);
        if (createResp.Status >= 400)
            throw new InvalidOperationException($"Joining group as guest failed with {createResp.Status}: {await createResp.TextAsync()}");

        var session = (await createResp.JsonAsync())?.Deserialize<GuestSessionResponse>(JsonDefaults.Options);
        if (session is null)
            throw new InvalidOperationException("Failed to deserialize guest session response.");

        actor.Remember(guestName, session);

        if (!string.IsNullOrEmpty(dishId))
        {
            var addResp = await api.AddGuestDishAsync(session.SessionId, dishId, quantity, session.SessionToken);
            if (addResp.Status >= 400)
                throw new InvalidOperationException($"Adding dish failed with {addResp.Status}: {await addResp.TextAsync()}");
        }

        if (submitCart)
        {
            var subResp = await api.SubmitGuestCartAsync(session.SessionId, session.SessionToken);
            if (subResp.Status >= 400)
                throw new InvalidOperationException($"Submitting cart failed with {subResp.Status}: {await subResp.TextAsync()}");
        }
    }
}
