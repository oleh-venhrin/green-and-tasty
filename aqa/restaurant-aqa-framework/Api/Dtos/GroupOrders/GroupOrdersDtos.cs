namespace Restaurant_Aqa_Framework.Api.Dtos.GroupOrders;

public sealed record InviteTokenResponse(string Token);

public sealed record GuestInviteResponse(
    string GroupReservationId,
    string LocationId,
    string RestaurantName,
    string LocationAddress,
    DateTime StartTime,
    DateTime EndTime,
    int CurrentGuestCount,
    int MaxGuestCount
);

public sealed record CreateGuestSessionRequest(
    string GroupReservationId,
    string GuestName
);

public sealed record GuestSessionResponse(
    string SessionId,
    string SessionToken
);

public sealed record AddGuestDishRequest(
    string DishId,
    int Quantity
);

public sealed record GroupDishShortDto(
    string Id,
    string Name,
    decimal Price,
    string? ImageUrl = null,
    string? Weight = null
);

public sealed record GroupOrderItemDto(
    GroupDishShortDto Dish,
    int Quantity
);

public sealed record ParticipantDto(
    string Id,
    string Type,
    string Name,
    IReadOnlyList<GroupOrderItemDto> Items,
    decimal Total,
    bool IsSubmitted
);

public sealed record GroupSessionsResponseDto(
    IReadOnlyList<ParticipantDto> Participants,
    decimal Total
);

public sealed record GroupStatusResponseDto(
    int CurrentParticipants,
    int MaxCapacity,
    string Status
);

public sealed record SubmitGroupPreOrderRequest(
    string ReservationId
);
