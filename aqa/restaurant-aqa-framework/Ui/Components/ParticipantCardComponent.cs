using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui.Components;

public sealed class ParticipantCardComponent(ILocator root)
{
    public ILocator Root => root;
    public ILocator Title => root.Locator("h2.title");
    public ILocator StatusBadge => root.Locator(".status-badge");
    public ILocator Subtotal => root.Locator(".subtotal-amount");
    public ILocator RemoveButton => root.Locator("button.remove-guest-btn");
    public ILocator Dishes => root.Locator("ul.dish-list > li.dish-item");

    public async Task RemoveAsync()
    {
        await RemoveButton.ClickAsync();
        var confirmButton = Root.Page.Locator("app-modal button.btn--danger");
        try
        {
            await confirmButton.WaitForAsync(new() { Timeout = 5000 });
            await confirmButton.ClickAsync();
        }
        catch
        {
            // Modal did not appear or already dismissed
        }
    }
}
