namespace DraftLite.Service.Interfaces;

public interface IAppMapper
{
    TDestination Map<TSource, TDestination>(TSource source);
}

