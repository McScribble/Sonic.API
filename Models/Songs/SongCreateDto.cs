namespace Sonic.Models;

public class SongCreateDto : GenericCreateEntityDto
{
    public required string Artist { get; set; }
    public required string Album { get; set; }
    public List<ExternalSource> ExternalSources { get; set; } = new();

    // ✅ Use actual entity relationships instead of ID lists
    public List<InstrumentDto> RequiredInstruments { get; set; } = new();
    public List<InstrumentDto> OptionalInstruments { get; set; } = new();
}