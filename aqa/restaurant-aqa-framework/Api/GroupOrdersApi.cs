using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Api.Dtos.GroupOrders;

namespace Restaurant_Aqa_Framework.Api;

public sealed class GroupOrdersApi(IAPIRequestContext api) : ApiClientBase(api)
{
    public Task<IAPIResponse> GetInviteTokenAsync(string reservationId, string? bearerToken = null) =>
        GetAsync(Url($"/reservations/{reservationId}/invite-token"), new APIRequestContextOptions
        {
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> GetInviteAsync(string token) =>
        GetAsync(Url($"/guest/invites/{token}"));

    public Task<IAPIResponse> CreateGuestSessionAsync(string groupReservationId, string guestName) =>
        Api.PostAsync(Url("/guest/sessions"), new APIRequestContextOptions
        {
            DataString = Serialize(new CreateGuestSessionRequest(groupReservationId, guestName)),
            Headers = Headers(),
        });

    public Task<IAPIResponse> AddGuestDishAsync(
        string guestSessionId, string dishId, int quantity, string sessionToken) =>
        Api.PostAsync(Url($"/guest/sessions/{guestSessionId}/dishes"), new APIRequestContextOptions
        {
            DataString = Serialize(new AddGuestDishRequest(dishId, quantity)),
            Headers = Headers(sessionToken),
        });

    public Task<IAPIResponse> SubmitGuestCartAsync(string guestSessionId, string sessionToken) =>
        Api.PostAsync(Url($"/guest/sessions/{guestSessionId}/submit"), new APIRequestContextOptions
        {
            DataString = "{}",
            Headers = Headers(sessionToken),
        });

    public Task<IAPIResponse> GetGroupSessionsAsync(string groupReservationId, string? bearerToken = null) =>
        GetAsync(Url($"/guest/groups/{groupReservationId}/sessions"), new APIRequestContextOptions
        {
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> GetGroupStatusAsync(string groupReservationId) =>
        GetAsync(Url($"/guest/groups/{groupReservationId}/status"));

    public Task<IAPIResponse> SubmitGroupPreOrderAsync(
        string groupReservationId, string reservationId, string? bearerToken = null) =>
        Api.PostAsync(Url($"/group-reservations/{groupReservationId}/submit"), new APIRequestContextOptions
        {
            DataString = Serialize(new SubmitGroupPreOrderRequest(reservationId)),
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> RemoveGuestSessionAsync(
        string groupReservationId, string guestSessionId, string? bearerToken = null) =>
        DeleteAsync(Url($"/guest/groups/{groupReservationId}/sessions/{guestSessionId}"), new APIRequestContextOptions
        {
            Headers = Headers(bearerToken),
        });
}
