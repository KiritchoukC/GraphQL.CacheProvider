
namespace GraphQL.CacheProvider.Delegates
{
    public delegate void OutAction<T1, T2, T3>(T1 type1, T2 type2, out T3 type3);
    public delegate void OutAction<T1, T2>(T1 type1, out T2 type2);
}
