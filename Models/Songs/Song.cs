using System.ComponentModel.DataAnnotations.Schema;

namespace Sonic.Models;

public class Song : GenericEntity
{
    public required string Artist { get; set; }
    public required string Album { get; set; }
    public List<ExternalSource> ExternalSources { get; set; } = new List<ExternalSource>();

    public List<Instrument> RequiredInstruments { get; set; } = new List<Instrument>();
    public List<Instrument> OptionalInstruments { get; set; } = new List<Instrument>();
}