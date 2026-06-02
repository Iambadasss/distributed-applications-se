namespace PersonalFinanceTracker.Models;

public enum RecordType
{
    income,
    expense
}

public enum CategoryType
{
    income,
    expense,
    subscription,
    saving
}

public enum SubscriptionStatus
{
    active,
    paused,
    cancelled,
    expired
}

public enum RenewalCycle
{
    weekly,
    monthly,
    quarterly,
    yearly,
    custom
}

public enum SavingsGoalStatus
{
    active,
    completed,
    cancelled
}

public enum CurrencyCode
{
    BGN,
    EUR,
    USD
}

public enum UserRole
{
    user,
    admin
}
