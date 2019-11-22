using System;
using System.Collections.Generic;
using System.Linq;

public static class IocServiceContainer
{
    private static Dictionary<Type, Dictionary<int, object>> repositoryIdentifiedService = new Dictionary<Type, Dictionary<int, object>>();
    private static Dictionary<Type, object> repositorySingleService = new Dictionary<Type, object>();

    public static List<T> GetAll<T>()
    {
        Type key = typeof(T);
        return (repositoryIdentifiedService.ContainsKey(key) ? repositoryIdentifiedService[key].Values.OfType<T>().ToList<T>() : new List<T>());
    }

    public static T GetService<T>() where T : class
    {
        Type key = typeof(T);
        if (!repositorySingleService.ContainsKey(key))
        {
            throw new ArgumentException($"service " + key +" not registered");
        }
        return (T)repositorySingleService[key];
    }

    public static T GetService<T>(int serviceId) where T : class
    {
        Type key = typeof(T);
        if (!repositoryIdentifiedService.ContainsKey(key))
        {
            throw new ArgumentException($"service of type "+ key +" not registered");
        }
        if (!repositoryIdentifiedService[key].ContainsKey(serviceId))
        {
            throw new ArgumentException($"service of type "+ key +", id={serviceId} not registered");
        }
        return (T)repositoryIdentifiedService[key][serviceId];
    }

    public static int GetServiceID<T>(T service)
    {
        Type key = typeof(T);
        if (!repositoryIdentifiedService.ContainsKey(key))
        {
            throw new ArgumentException($"service of type "+ key +" not registered");
        }
        if (!repositoryIdentifiedService[key].ContainsValue(service))
        {
            throw new ArgumentException(string.Format("service instance not present", new object[0]));
        }
        return repositoryIdentifiedService[key].First<KeyValuePair<int, object>>(x => x.Value.Equals(service)).Key;
    }

    public static void RegisterService<T>(T service) where T : class
    {
        Type key = typeof(T);
        if (!repositorySingleService.ContainsKey(key))
        {
            repositorySingleService.Add(key, service);
        }
    }

    public static void RegisterService<T>(int serviceId, T service) where T : class
    {
        Type key = typeof(T);
        if (!repositoryIdentifiedService.ContainsKey(key))
        {
            repositoryIdentifiedService.Add(key, new Dictionary<int, object>());
        }
        repositoryIdentifiedService[key].Add(serviceId, service);
    }

    public static void UnregisterService<T>() where T : class
    {
        Type key = typeof(T);
        if (!repositorySingleService.ContainsKey(key))
        {
            throw new ArgumentException($"service "+ key +" not registered");
        }
        repositorySingleService.Remove(key);
    }

    public static void UnregisterService<T>(int serviceId) where T : class
    {
        Type key = typeof(T);
        if (!repositoryIdentifiedService.ContainsKey(key))
        {
            throw new ArgumentException($"service of type "+ key +" not registered");
        }
        if (!repositoryIdentifiedService[key].ContainsKey(serviceId))
        {
            throw new ArgumentException($"service of type "+ key +", id={serviceId} not registered");
        }
        repositoryIdentifiedService[key].Remove(serviceId);
    }
}
