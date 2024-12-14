using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Persistance;
internal class HeadQuartersContestService : IHeadQuartersContestService
{

    private readonly IDbContextFactory<SocialDbContext> _contextFactory;

    public HeadQuartersContestService(IDbContextFactory<SocialDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<byte[]?> LoadSoundBytes(string soundId)
    {
        await using var cont = await _contextFactory.CreateDbContextAsync();
        var returnee = await cont.Sounds
            .FirstOrDefaultAsync(_ => _.SoundId == soundId);
        return returnee?.SoundBytes;
    }
}
