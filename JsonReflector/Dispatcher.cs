
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace JsonReflector
{

    public class ParameterDescrictor
    {
        public object Def
        {
            get;
            set;
        }
}
    // our lookup table has these
    public class ClassEntry
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        // public object Instance { get; set; }

        private static string DescribeType(Type t)
        {
            if (!t.IsGenericType)
            {
                return t.Name;
            }
            return t.Name + " " + String.Join(' ', t.GetGenericArguments().Select(a => DescribeType(a)));
        }

        private static List<object> DescribeParameter(ParameterInfo pi)
        {
            ParameterDescrictor pd = null;   
            if (pi.HasDefaultValue)
            {
                pd = new ParameterDescrictor
                {
                    Def = pi.DefaultValue
                };
            }

            var res = new List<object>();
            res.Add(pi.Name);
            res.Add(DescribeType(pi.ParameterType));
            
            if (pd != null)
            {
                res.Add(pd);
            }

            return res;
        }
        
        public List<object> Describe()
        {
            List<object> all = new();
            all.Add(Name);
            all.Add(Type.FullName);

            foreach (var method in Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static))
            {
                var paramss = method.GetParameters().Select(p => DescribeParameter(p) );
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
            var ctors = t.GetConstructors();
            if (!ctors.Any())
            {
                throw new Exception($"No ctor for {t}");
            }

            var ctor = ctors[0];
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

    public class ReturnValue
    {
        public object R { get; set; }
        public string Exc { get; set; }
        public string Out { get; set; }
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
            var methodInfo = registration.Type.GetMethod(header.methodName);
            var args = PopulateArguments(methodInfo, ref rd);

            ReturnValue retVal = new();
            var savedOut = Console.Out;
            var outWriter = new StringWriter();

            try
            {
                var instance = methodInfo.IsStatic ? null : session.GetInstance(registration.Type);
                Console.SetOut(outWriter);
                retVal.R = methodInfo.Invoke(instance, args); 
            }
            catch (Exception e)
            {
                retVal.Exc = e.InnerException?.ToString() ?? e.ToString();
            }
            finally
            {
                Console.SetOut(savedOut);
                retVal.Out = outWriter.ToString();
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