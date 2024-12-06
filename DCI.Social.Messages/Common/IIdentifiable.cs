using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Common;
public interface IIdentifiable
{
    public long Id { get; }


    public static IdentityGenerator WithId => new IdentityGenerator();

}

public class IdentityGenerator
{
    private long _id;
    private readonly object _lock = new { };
    public long NextId()
    {
        lock(_lock)
        {
            return ++_id;
        }
    }

}

