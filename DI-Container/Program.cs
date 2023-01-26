#region Run

TestSingletonInstances();
TestScopedInstances();
TestTranInstances();

static void TestScopedInstances()
{
    var container = new CustomDIContainer();
    container.RegisterScoped<IService, Service>();

    Console.WriteLine("scoped 1 start");

    container.BeginScope();
    var scoped1 = container.Resolve<IService>();
    var scoped2 = container.Resolve<IService>();
    scoped1.Show();
    scoped2.Show();
    container.EndScope();
    Console.WriteLine("scoped 1 end ");

    Console.WriteLine();

    Console.WriteLine("scoped 2 start");

    container.BeginScope();
    var scoped3 = container.Resolve<IService>();
    var scoped4 = container.Resolve<IService>();
    scoped3.Show();
    scoped4.Show();
    container.EndScope();

    Console.WriteLine("scoped 2 end");
    Console.WriteLine();



}
static void TestSingletonInstances()
{
    var container = new CustomDIContainer();
    container.RegisterSingleton<IService, Service>();

    Console.WriteLine("Singleton start");

    var s1 = container.Resolve<IService>();
    var s2 = container.Resolve<IService>();
    var s3 = container.Resolve<IService>();
    s1.Show();
    s2.Show();
    s3.Show();

    Console.WriteLine("Singleton  end ");

    Console.WriteLine();


}

static void TestTranInstances()
{
    var container = new CustomDIContainer();
    container.Register<IService, Service>();

    Console.WriteLine("Transient start");

    var s1 = container.Resolve<IService>();
    var s2 = container.Resolve<IService>();
    var s3 = container.Resolve<IService>();
    s1.Show();
    s2.Show();
    s3.Show();

    Console.WriteLine("Transient end ");

    Console.WriteLine();
}
#endregion



#region DIContainer
public class CustomDIContainer
{
    private readonly Dictionary<Type, Type> _registeredTypes = new();
    private readonly Dictionary<Type, object> _singletonInstances = new();
    private readonly Stack<Dictionary<Type, object>> _scopedInstances = new();

    public void Register<TAbstraction, TImplementation>() where TImplementation : TAbstraction
    {
        _registeredTypes[typeof(TAbstraction)] = typeof(TImplementation);
    }

    public void RegisterSingleton<TAbstraction, TImplementation>() where TImplementation : TAbstraction
    {
        _registeredTypes[typeof(TAbstraction)] = typeof(TImplementation);
        _singletonInstances[typeof(TAbstraction)] = null!;
    }

    public void RegisterScoped<TAbstraction, TImplementation>() where TImplementation : TAbstraction
    {
        _registeredTypes[typeof(TAbstraction)] = typeof(TImplementation);
    }

    public void BeginScope()
    {
        _scopedInstances.Push(new Dictionary<Type, object>());
    }

    public void EndScope()
    {
        _scopedInstances.Pop();

    }

    public TAbstraction Resolve<TAbstraction>()
    {
        var type = _registeredTypes[typeof(TAbstraction)];
        return GetInstance<TAbstraction>(type);
    }

    private TAbstraction GetInstance<TAbstraction>(Type type)
    {
        bool isSingleton = _singletonInstances.ContainsKey(typeof(TAbstraction));
        if (isSingleton) return GetSingletonInstance<TAbstraction>(type);

        bool isScoped = _scopedInstances.Any();
        if (isScoped) return GetScopedInstance<TAbstraction>(type);

        return CreateInstance<TAbstraction>(type);
    }

    private TAbstraction GetScopedInstance<TAbstraction>(Type type)
    {
        if (!_scopedInstances.Peek().ContainsKey(typeof(TAbstraction)))
        {
            _scopedInstances.Peek()[typeof(TAbstraction)] = CreateInstance<TAbstraction>(type)!;
        }
        return (TAbstraction)_scopedInstances.Peek()[typeof(TAbstraction)];
    }

    private TAbstraction GetSingletonInstance<TAbstraction>(Type type)
    {
        if (_singletonInstances[typeof(TAbstraction)] is null)
        {
            _singletonInstances[typeof(TAbstraction)] = CreateInstance<TAbstraction>(type)!;
        }
        return (TAbstraction)_singletonInstances[typeof(TAbstraction)];
    }

    private static TAbstraction CreateInstance<TAbstraction>(Type type)
    {
        var constructor = type.GetConstructors().FirstOrDefault();
        var parameters = constructor!.GetParameters();
        var parameterInstances = new List<object>();
        foreach (var parameter in parameters)
        {
            parameterInstances.Add(Activator.CreateInstance(parameter.ParameterType)!);
        }
        return (TAbstraction)constructor.Invoke(parameterInstances.ToArray());
    }
}
#endregion


#region Service
public class Service : IService
{
    private readonly DateTime _dateTime;
    public Service(GenerateData generateData)
    {
        _dateTime = generateData.DateTime;
    }
    public void Show() => Console.WriteLine(_dateTime.Ticks);
}
public interface IService
{
    void Show();
}
public record GenerateData
{
    public DateTime DateTime => DateTime.Now;

}
#endregion