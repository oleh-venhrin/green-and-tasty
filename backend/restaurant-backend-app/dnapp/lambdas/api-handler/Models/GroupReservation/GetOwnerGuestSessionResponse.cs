using System.Text.Json.Serialization;

namespace APIHandler.Models.GroupReservation;

public record GetOwnerGuestSessionResponse(
    [property: JsonPropertyName("guestSessionId")] string GuestSessionId);
