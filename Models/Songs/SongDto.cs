using System.ComponentModel.DataAnnotations.Schema;
using Sonic.API.Models.Instruments;

namespace Sonic.Models;

public class SongDto : GenericEntity
{
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public List<ExternalSource> ExternalSources { get; set; } = new List<ExternalSource>();
    public List<InstrumentSummaryDto> RequiredInstruments { get; set; } = new List<InstrumentSummaryDto>();
    public List<InstrumentSummaryDto> OptionalInstruments { get; set; } = new List<InstrumentSummaryDto>();
}