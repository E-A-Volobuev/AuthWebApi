namespace AuthCookie.Policies
{
    public static class PolicyNames
    {
        /// <summary>Политика: у пользователя должно быть claim "department" со значением "finance".</summary>
        public const string FinanceDepartment = "FinanceDepartment";

        /// <summary>Политика: у пользователя должно быть claim "BranchId" со значением "B001".</summary>
        public const string CanViewBranchB001 = "CanViewBranchB001";
    }
}
