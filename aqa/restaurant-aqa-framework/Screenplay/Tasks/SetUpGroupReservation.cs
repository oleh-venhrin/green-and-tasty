using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.GroupOrders;
using Restaurant_Aqa_Framework.Api.Dtos.Reservations;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class SetUpGroupReservation(string? reservationId = null) : ITask
{
    public const string DefaultReservationId = "res-demo-01";
    public const string DefaultGroupId = "grp-demo-01";
    public const string DefaultToken = "token-demo-12345";

    public async Task PerformAsAsync(Actor actor)
    {
        var resId = reservationId ?? DefaultReservationId;

        var res = new ReservationResponse(
            resId,
            "table-03",
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(2),
            10,
            ReservationStatus.Reserved,
            "5861b310-60e1-700f-ca31-b915eb410490",
            "waiter-01",
            "12 Rustaveli Avenue, Tbilisi",
            "Green & Tasty",
            false);

        actor.Remember(res);
        actor.Remember(new InviteTokenResponse(DefaultToken));
        actor.Remember(new GuestInviteResponse(
            DefaultGroupId,
            "loc-tbilisi-01",
            "Green & Tasty",
            "12 Rustaveli Avenue, Tbilisi",
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(2),
            3,
            10));

        await Task.CompletedTask;
    }
}
