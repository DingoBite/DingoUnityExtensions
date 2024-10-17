namespace DingoUnityExtensions.Utils.Search
{
    public interface IFilterable
    {
        public void ReFilter();
        public void Filter(string nameFilter);
    }
}