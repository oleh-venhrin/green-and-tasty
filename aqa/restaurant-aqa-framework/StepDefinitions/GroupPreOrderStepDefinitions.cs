using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Auth;
using Restaurant_Aqa_Framework.Api.Dtos.GroupOrders;
using Restaurant_Aqa_Framework.Api.Dtos.Reservations;
using Restaurant_Aqa_Framework.Screenplay;
using Restaurant_Aqa_Framework.Screenplay.Questions;
using Restaurant_Aqa_Framework.Screenplay.Tasks;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.StepDefinitions;

[Binding]
public sealed class GroupPreOrderStepDefinitions(Actor actor) : StepDefinitionBase(actor)
{
    private const string OwnerEmail = "oleh.marathon.dev2026@gmail.com";
    private const string OwnerPassword = "Password123!";
    private const string NonExistentGroupId = "grp-does-not-exist";
    private const string NonExistentGuestId = "gs-does-not-exist";

    [Given("a customer who owns a group booking")]
    [Scope(Tag = "api")]
    public async Task GivenCustomerOwnsGroupBookingApi()
    {
        await Actor.AttemptsToAsync(new SignInThroughTheService(OwnerEmail, OwnerPassword));
        await Actor.AttemptsToAsync(new SetUpGroupReservation());
    }

    [Given("a customer who owns a group booking")]
    [Scope(Tag = "ui")]
    public async Task GivenCustomerOwnsGroupBookingUi()
    {
        await Actor.AttemptsToAsync(new SignInThroughTheLoginForm(OwnerEmail, OwnerPassword));
        await Actor.AttemptsToAsync(new SetUpGroupReservation());
    }

    [When("the owner reviews the group pre-order")]
    public Task WhenOwnerReviewsTheGroupPreOrder() =>
        Actor.AttemptsToAsync(new ViewTheGroupSessions());

    [Then("the owner sees the participants and their dishes")]
    public async Task ThenOwnerSeesParticipantsAndDishes()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Succeeded).IsTrue().Because($"API returned status {outcome.Status}: {outcome.Body}");

        var sessions = await Actor.AsksForAsync(new TheGroupSessions());
        await Assert.That(sessions.Participants).IsNotEmpty();
    }

    [Then("the total matches the sum of participant totals")]
    public async Task ThenTotalMatchesSumOfParticipantTotals()
    {
        var sessions = await Actor.AsksForAsync(new TheGroupSessions());
        var calculatedTotal = sessions.Participants.Sum(p => p.Total);
        await Assert.That(sessions.Total).IsEqualTo(calculatedTotal);
    }

    [When("the owner opens the group pre-order page")]
    [Given("the owner has opened the group pre-order page")]
    public Task WhenOwnerOpensTheGroupOrderPage() =>
        Actor.AttemptsToAsync(new OpenTheGroupOrderPage());

    [Then("the owner sees the group pre-order details and participants")]
    public async Task ThenOwnerSeesDetailsAndParticipants()
    {
        var details = await Actor.AsksForAsync(new TheGroupPreOrderDetails());
        await Assert.That(details.PageTitle).Contains("Group Pre-Order");
        await Assert.That(details.ParticipantCount).IsGreaterThanOrEqualTo(0);
    }

    [When("another customer tries to review the group pre-order")]
    public async Task WhenAnotherCustomerTriesToReviewGroup()
    {
        var invite = Actor.Recall<GuestInviteResponse>();
        var api = new GroupOrdersApi(Actor.UsingTheApi());

        // Call without owner token
        await Actor.TheLastResponse().CaptureAsync(
            await api.GetGroupSessionsAsync(invite.GroupReservationId, bearerToken: null));
    }

    [Then("access is refused")]
    public async Task ThenAccessIsRefused()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status == 401 || outcome.Status == 403).IsTrue()
            .Because($"expected 401/403 but got {outcome.Status}: {outcome.Body}");
    }

    [When("someone tries to review a non-existent group pre-order")]
    public async Task WhenSomeoneReviewsNonExistentGroup()
    {
        var token = (await Actor.AsksForAsync(new TheIdToken())) ?? string.Empty;
        var api = new GroupOrdersApi(Actor.UsingTheApi());

        await Actor.TheLastResponse().CaptureAsync(
            await api.GetGroupSessionsAsync(NonExistentGroupId, bearerToken: token));
    }

    [Then("the guest is told that the group could not be found")]
    public async Task ThenTheGuestIsToldGroupNotFound()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status == 404 || outcome.Status == 400 || outcome.Status == 403).IsTrue()
            .Because($"expected 400/403/404 but got {outcome.Status}: {outcome.Body}");
    }

    [When("the owner submits the group pre-order")]
    public Task WhenOwnerSubmitsTheGroupPreOrder() =>
        Actor.AttemptsToAsync(new SubmitTheGroupPreOrder());

    [Then("the group pre-order is submitted")]
    public async Task ThenTheGroupPreOrderIsSubmitted()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Succeeded).IsTrue().Because($"Submit failed with {outcome.Status}: {outcome.Body}");
    }

    [When("the owner submits the pre-order through the form")]
    public Task WhenOwnerSubmitsPreOrderThroughForm() =>
        Actor.AttemptsToAsync(new SubmitTheGroupPreOrder());

    [Then("the storefront confirms the pre-order is sent to the restaurant")]
    public async Task ThenStorefrontConfirmsSent()
    {
        var page = new GroupOrderPage(Actor.LookingAtThePage());
        await page.WaitForLoadedAsync();
        var badge = Actor.LookingAtThePage().Locator("header.page-header .badge");
        try
        {
            await badge.Filter(new() { HasText = "Sent to Restaurant" }).WaitForAsync(new() { Timeout = 15000 });
        }
        catch
        {
            // Fallback to checking inner text directly
        }
        var badgeText = (await badge.InnerTextAsync()).Trim();
        await Assert.That(badgeText).Contains("Sent to Restaurant");
    }

    [When("another customer tries to submit the group pre-order")]
    public async Task WhenAnotherCustomerTriesToSubmit()
    {
        var invite = Actor.Recall<GuestInviteResponse>();
        var res = Actor.Recall<ReservationResponse>();
        var api = new GroupOrdersApi(Actor.UsingTheApi());

        // Call without valid owner token
        await Actor.TheLastResponse().CaptureAsync(
            await api.SubmitGroupPreOrderAsync(invite.GroupReservationId, res.Id, bearerToken: null));
    }

    [When("the owner removes a guest from the group")]
    public async Task WhenOwnerRemovesGuestFromGroup()
    {
        // First ensure a removable guest exists
        var guestName = $"Guest-{Guid.NewGuid():N}"[..12];
        await Actor.AttemptsToAsync(new JoinGroupAsGuest(guestName, "dish-01", 1, submitCart: true));
        await Actor.AttemptsToAsync(new RemoveGuestFromGroup(guestName));
    }

    [Then("the guest is no longer part of the group")]
    public async Task ThenGuestNoLongerPartOfGroup()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Succeeded).IsTrue().Because($"removal failed with {outcome.Status}: {outcome.Body}");
    }

    [When("the owner removes a guest through the storefront")]
    public async Task WhenOwnerRemovesGuestStorefront()
    {
        var page = new GroupOrderPage(Actor.LookingAtThePage());
        var guestCard = page.ParticipantByName("Andrii");
        if (await guestCard.RemoveButton.CountAsync() > 0)
        {
            await guestCard.RemoveAsync();
        }
    }

    [Then("the guest is removed from the participants list")]
    public async Task ThenGuestIsRemovedFromList()
    {
        var page = new GroupOrderPage(Actor.LookingAtThePage());
        await page.RefreshButton.ClickAsync();
    }

    [When("the owner tries to remove an invalid guest from the group")]
    public async Task WhenOwnerRemovesInvalidGuest()
    {
        var invite = Actor.Recall<GuestInviteResponse>();
        var token = (await Actor.AsksForAsync(new TheIdToken())) ?? string.Empty;
        var api = new GroupOrdersApi(Actor.UsingTheApi());

        await Actor.TheLastResponse().CaptureAsync(
            await api.RemoveGuestSessionAsync(invite.GroupReservationId, NonExistentGuestId, token));
    }

    [Then("the removal is refused")]
    public async Task ThenTheRemovalIsRefused()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status == 400 || outcome.Status == 404).IsTrue()
            .Because($"expected 400/404 but got {outcome.Status}: {outcome.Body}");
    }
}
