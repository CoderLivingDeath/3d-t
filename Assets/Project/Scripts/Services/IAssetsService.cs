using _3dt.Infrastructure;

public interface IAssetsService
{
    IResult<T> Get<T>(string keyOrPath);
}
