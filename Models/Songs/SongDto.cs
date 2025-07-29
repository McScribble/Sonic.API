using System.ComponentModel.DataAnnotations.Schema;

namespace Sonic.Models;

public class SongDto : GenericEntity
{
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public List<ExternalSource> ExternalSources { get; set; } = new List<ExternalSource>();
    public List<InstrumentDto> RequiredInstruments { get; set; } = new List<InstrumentDto>();
    public List<InstrumentDto> OptionalInstruments { get; set; } = new List<InstrumentDto>();
}