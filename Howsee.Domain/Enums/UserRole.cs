namespace Howsee.Domain.Enums;

public enum UserRole
{
    Administrator = 1,
    Contractor = 2,
    Agency = 3,
    Buyer = 4
}

public static class UserRoleExtensions
{
    public static string ToStringValue(this UserRole role)
    {
        return role switch
        {
            UserRole.Administrator => "Administrator",
            UserRole.Contractor => "Contractor",
            UserRole.Agency => "Agency",
            UserRole.Buyer => "Buyer",
            _ => "Buyer"
        };
    }
}
