using System;
using Sonic.Models;
using Microsoft.EntityFrameworkCore;

namespace Sonic.API.Services;

public class SongIncludeConfiguration : IIncludeConfiguration<Song>
{
    public IQueryable<Song> ApplyIncludes(IQueryable<Song> query)
    {
        return query
            .Include(s => s.RequiredInstruments)
            .Include(s => s.OptionalInstruments);
    }
}
