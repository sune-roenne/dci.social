using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Buzzer;
public record Buzz(
    string User,
    DateTime BuzzTime
    );
