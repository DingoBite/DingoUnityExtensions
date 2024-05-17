namespace DingoUnityExtensions.MonoBehaviours.UI.SearchDropdownLayoutBased
{
    public struct SearchDropdownValue
    {
        public static bool IsNull(SearchDropdownValue searchDropdownValue) => searchDropdownValue.Equals(Null);
        public static SearchDropdownValue Null => new(-1, "None");
        
        public readonly int Id;
        public readonly string OptionName;
        public readonly string SelectionName;

        public SearchDropdownValue(int id, string optionName, string selectionName = null)
        {
            Id = id;
            OptionName = optionName;
            SelectionName = selectionName ?? optionName;
        }
    }
}