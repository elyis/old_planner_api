using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace old_planner_api.src.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TaskType
    {
        Meeting,
        Task,
        Reminder,
        Inform
    }
}