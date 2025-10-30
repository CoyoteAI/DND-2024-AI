namespace DnDAI.Core.Enums;

public enum RechargeType
{
    None,           // No recharge needed (passive feature)
    ShortRest,      // Recharges on short or long rest
    LongRest,       // Recharges only on long rest
    Dawn,           // Recharges at dawn
    Other           // Special recharge condition
}
