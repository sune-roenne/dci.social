using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Common;
public abstract record AbstractMessage : IIdentifiable
{
    private static readonly IdentityGenerator IdGen = IIdentifiable.WithId;
    private long _id = IdGen.NextId();
    public long Id => _id;

}
