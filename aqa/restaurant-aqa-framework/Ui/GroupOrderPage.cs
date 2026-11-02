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

    public async Task NavigateAsync(string reservationId)
    {
        await NavigateToAsync($"/reservations/{reservationId}/group-order");
        await WaitForLoadedAsync();
    }

    public async Task WaitForLoadedAsync()
    {
        try
        {
            if (await Page.Locator(".loading-state").CountAsync() > 0)
            {
                await Page.Locator(".loading-state").WaitForAsync(new() { State = WaitForSelectorState.Detached, Timeout = 10000 });
            }
        }
        catch
        {
            // Ignore if loading disappears quickly
        }

        try
        {
            await Page.Locator(".participants-section, .empty-state, .error-state").First.WaitForAsync(new() { Timeout = 10000 });
        }
        catch
        {
            // Page loaded without these sections
        }
    }

    public ParticipantCardComponent Participant(int index) => new(ParticipantCards.Nth(index));

    public ParticipantCardComponent ParticipantByName(string name) =>
        new(ParticipantCards.Filter(new() { HasText = name }));

    public async Task<int> ParticipantCountAsync()
    {
        await WaitForLoadedAsync();
        return await ParticipantCards.CountAsync();
    }

    public async Task SubmitPreOrderAsync()
    {
        await WaitForLoadedAsync();
        if (await SubmitButton.CountAsync() > 0 && await SubmitButton.IsEnabledAsync())
        {
            await SubmitButton.ClickAsync();
            await ModalConfirmButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            await ModalConfirmButton.ClickAsync();
            try
            {
                await Page.WaitForResponseAsync(r => r.Url.Contains("/submit"), new() { Timeout = 10000 });
            }
            catch
            {
                // In case the response completes very quickly
            }
        }
    }
}
