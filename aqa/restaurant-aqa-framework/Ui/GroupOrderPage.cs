using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Ui.Components;

namespace Restaurant_Aqa_Framework.Ui;

public sealed class GroupOrderPage(IPage page) : BasePage(page)
{
    public ILocator PageTitle => Page.Locator("header.page-header h1");
    public ILocator StatusBadge => Page.Locator("header.page-header .badge");
    public ILocator WarningBanner => Page.Locator(".warning-banner");
    public ILocator EmptyState => Page.Locator(".empty-state");
    public ILocator ConsolidatedSection => Page.Locator(".consolidated-section");
    public ILocator ConsolidatedItems => Page.Locator("ul.consolidated-list > li.consolidated-item");
    public ILocator GrandTotal => Page.Locator(".grand-total-row .total-amount");
    public ILocator SubmitButton => Page.Locator("button.btn-submit");
    public ILocator ModalConfirmButton => Page.Locator("app-modal button.btn--primary");
    public ILocator ModalCancelButton => Page.Locator("app-modal button.btn--secondary");
    public ILocator RefreshButton => Page.Locator("button.btn-refresh");

    private ILocator ParticipantCards => Page.Locator("ul.participants-list > li.participant-item app-participant-card");

    public Task NavigateAsync(string reservationId) => NavigateToAsync($"/reservations/{reservationId}/group-order");

    public ParticipantCardComponent Participant(int index) => new(ParticipantCards.Nth(index));

    public ParticipantCardComponent ParticipantByName(string name) =>
        new(ParticipantCards.Filter(new() { HasText = name }));

    public async Task<int> ParticipantCountAsync() => await ParticipantCards.CountAsync();

    public async Task SubmitPreOrderAsync()
    {
        await SubmitButton.ClickAsync();
        await ModalConfirmButton.ClickAsync();
    }
}
