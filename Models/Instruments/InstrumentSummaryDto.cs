using Sonic.Models;

namespace Sonic.API.Models.Instruments;

public class InstrumentSummaryDto : GenericEntity
{
    public new required string Name { get; set; }
    public new string? Emoji { get; set; }
}
