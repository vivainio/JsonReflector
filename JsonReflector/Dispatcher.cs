
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReflectorServer
{
    // our lookup table has these
    public class ClassEntry
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        // public object Instance { get; set; }

        private string DescribeType(Type t)
        {
            if (!t.IsGenericType)
            {
                return t.Name;
            }
            return t.Name + " " + String.Join(' ', t.GetGenericArguments().Select(a => DescribeType(a)));
        }

        public List<object> Describe()
        {
            List<object> all = new();
            all.Add(Name);
            all.Add(Type.FullName);

            foreach (var method in Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var paramss = method.GetParameters().Select(p => $"{p.Name} {DescribeType(p.ParameterType)}");
                all.Add(new object[] { method.Name, paramss });

            }
            return all;

        }
    }

    public interface IDispatcherIntegration
    {
        // this is the place to call setinstance & create initial services
        Session CreateSession();
    }

    public class Session
    {
        Dictionary<Type, object> Instances = new();
        public object GetInstance(Type T)
        {

            // already instantiated
            var t = T;
            if (Instances.ContainsKey(t)) {
                return Instances[t];
            }

            // minimal di - we support one constructor
            var ctor = t.GetConstructors()[0];
            var cparams = ctor.GetParameters().Select(param => GetInstance(param.ParameterType)).ToArray();
            var invoked = ctor.Invoke(cparams);
            Instances[t] = invoked;
            return invoked;
        }

        public void SetInstance<T>(T instance)
        {
            Instances.Add(typeof(T), instance);
        }

    

    }
    public class Dispatcher
    {
        public Dictionary<string, Session> Sessions = new();
        Dictionary<string, ClassEntry> TypeMap = new();
        // more variants of RegisterTypes possibly needed, e.g. add with full
        // namespace or own name

        public void RegisterTypes(IEnumerable<Type> types, string prefix = "")
        {
            foreach (var t in types)
            {
                var name = prefix + t.Name;
                TypeMap.Add(name, new ClassEntry
                {
                    Name = name,
                    Type = t
                });
            }
        }

        public byte[] Describe()
        {
            var all = TypeMap.Values.Select(p => p.Describe()).ToArray();
            return JsonSerializer.SerializeToUtf8Bytes(all, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        public static (string className, string methodName) ReadHeader(ref Utf8JsonReader rd)
        {
            rd.Read(); // startarray
            rd.Read();
            var className = rd.GetString();
            rd.Read();
            var methodName = rd.GetString();
            return (className, methodName);

        }
        public byte[] DispatchJson(ReadOnlySpan<byte> json, Session session)
        {
            var rd = new Utf8JsonReader(json);
            var header = ReadHeader(ref rd);
            var registration = TypeMap[header.className];
            var instance = session.GetInstance(registration.Type);
            var methodInfo = registration.Type.GetMethod(header.methodName);
            var args = PopulateArguments(methodInfo, ref rd);

            object retVal;
            try
            {
                retVal = methodInfo.Invoke(instance, args);

            } catch (Exception e)
            {
                retVal = new
                {
                    Exception = e.InnerException.ToString()
                };

            }
            return JsonSerializer.SerializeToUtf8Bytes(retVal);
        }


        public static object[] PopulateArguments(MethodInfo targetMethod, ref Utf8JsonReader rd)
        {

            var objects = new List<object>();
            foreach (var par in targetMethod.GetParameters())
            {
                rd.Read();
                var extracted = JsonSerializer.Deserialize(ref rd, par.ParameterType);
                objects.Add(extracted);
            }
            return objects.ToArray();

        }

    }
}