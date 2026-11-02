using Restaurant_Aqa_Framework.Api.Dtos.Reservations;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class OpenTheGroupOrderPage(string? reservationId = null) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var resId = reservationId ?? actor.Recall<ReservationResponse>().Id;
        var page = new GroupOrderPage(actor.LookingAtThePage());
        await page.NavigateAsync(resId);
    }
}
