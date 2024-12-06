namespace DCI.Social.FOB.Common;

public record IdempotentActionComitted(
    long MessageId,
    DateTime CommittedTime
    );