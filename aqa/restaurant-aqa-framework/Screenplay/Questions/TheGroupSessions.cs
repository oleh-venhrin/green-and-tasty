using Restaurant_Aqa_Framework.Api.Dtos.GroupOrders;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

public sealed class TheGroupSessions : IQuestion<GroupSessionsResponseDto>
{
    public Task<GroupSessionsResponseDto> AnsweredByAsync(Actor actor)
    {
        var response = actor.TheLastResponse();
        return Task.FromResult(response.Json<GroupSessionsResponseDto>());
    }
}
