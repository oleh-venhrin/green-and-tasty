using System.Text.Json.Serialization;

namespace APIHandler.Models.GroupReservation;

public record SubmitGroupOrderDishResponse(
    [property: JsonPropertyName("dishId")] string DishId,
    [property: JsonPropertyName("quantity")] int Quantity);
