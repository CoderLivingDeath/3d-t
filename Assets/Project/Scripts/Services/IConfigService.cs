using _3dt.Infrastructure;

public interface IConfigService
{
    IResult<T> Get<T>(string keyOrPath);
}
