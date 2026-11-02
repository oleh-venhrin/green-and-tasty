using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

public sealed record GroupPreOrderUiDetails(
    string PageTitle,
    string StatusBadge,
    string GrandTotal,
    int ParticipantCount,
    bool HasWarningBanner
);

public sealed class TheGroupPreOrderDetails : IQuestion<GroupPreOrderUiDetails>
{
    public async Task<GroupPreOrderUiDetails> AnsweredByAsync(Actor actor)
    {
        var page = new GroupOrderPage(actor.LookingAtThePage());
        await page.PageTitle.WaitForAsync();

        var title = (await page.PageTitle.InnerTextAsync()).Trim();
        var badge = (await page.StatusBadge.InnerTextAsync()).Trim();
        var total = await page.GrandTotal.CountAsync() > 0 ? (await page.GrandTotal.InnerTextAsync()).Trim() : string.Empty;
        var count = await page.ParticipantCountAsync();
        var warning = await page.WarningBanner.CountAsync() > 0;

        return new GroupPreOrderUiDetails(title, badge, total, count, warning);
    }
}
