namespace ASTV.Services {
    public interface IEntityDefaultProvider<T, TKey> 
        where T: class, new()
       // where TKey: class, new()  
    {
        T GetDefault();
        T GetDefault(TKey key);
    }
}