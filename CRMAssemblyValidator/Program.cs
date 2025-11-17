using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Activities;
using System.Reflection;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;

namespace CRMAssemblyValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string path = args[0];
                if (File.Exists(path))
                {
                    List<string> problems = new List<string>();
                    Assembly assembly = Assembly.LoadFrom(Path.GetFullPath(path));
                    Console.WriteLine("Scanning " + assembly.GetName().Name);

                    problems.AddRange(CheckInputAndOutputAttributes(assembly));
                    problems.AddRange(CheckEntityReferenceHasTargets(assembly));
                    problems.AddRange(CheckArgumentTypes(assembly));

                    foreach (string line in problems)
                    {
                        Console.WriteLine(line);
                    }

                    if (problems.Count > 0)
                    {
                        Environment.Exit(1);
                    }
                }
            }
        }
        public static List<string> CheckInputAndOutputAttributes(Assembly input)
        {
            List<string> problems = new List<string>();
            foreach (Type assemblyType in input.ExportedTypes)
            {
                if (assemblyType.BaseType == typeof(CodeActivity))
                {
                    foreach (PropertyInfo propertyInfo in assemblyType.GetProperties())
                    {
                        if (propertyInfo.PropertyType.BaseType == typeof(InArgument) && !propertyInfo.GetCustomAttributes(typeof(InputAttribute)).Any())
                        {
                            problems.Add($"Plugin {assemblyType.FullName} InArgument {propertyInfo.Name} is missing an Input attribute");
                        }
                        else if (propertyInfo.PropertyType.BaseType == typeof(OutArgument) && !propertyInfo.GetCustomAttributes(typeof(OutputAttribute)).Any())
                        {
                            problems.Add($"Plugin {assemblyType.FullName} OutArgument {propertyInfo.Name} is missing an Output attribute");
                        }
                    }
                }
            }
            return problems;
        }

        public static List<string> CheckEntityReferenceHasTargets(Assembly input)
        {
            List<string> problems = new List<string>();
            foreach (Type assemblyType in input.ExportedTypes)
            {
                if (assemblyType.BaseType == typeof(CodeActivity))
                {
                    foreach (PropertyInfo propertyInfo in assemblyType.GetProperties())
                    {
                        if ((propertyInfo.PropertyType == typeof(InArgument<EntityReference>) 
                            ||  propertyInfo.PropertyType == typeof(OutArgument<EntityReference>)) 
                            && !propertyInfo.GetCustomAttributes(typeof(ReferenceTargetAttribute)).Any())
                        {
                            problems.Add($"Plugin {assemblyType.FullName} property {propertyInfo.Name} is an entity reference but is missing a reference target");
                        }
                    }
                }
            }
            return problems;
        }

        public static List<string> CheckArgumentTypes(Assembly input)
        {
            var problems = new List<string>();
            
            // Valid types for InArgument and OutArgument
            var validTypes = new Type[]
            {
                typeof(bool),
                typeof(DateTime),
                typeof(decimal),
                typeof(double),
                typeof(EntityReference),
                typeof(int),
                typeof(Money),
                typeof(OptionSetValue),
                typeof(string)
            };

            foreach (var assemblyType in input.ExportedTypes)
            {
                if (assemblyType.BaseType == typeof(CodeActivity))
                {
                    foreach (var propertyInfo in assemblyType.GetProperties())
                    {
                        var propertyType = propertyInfo.PropertyType;
                        
                        // Check if it's an InArgument or OutArgument
                        if (!propertyType.IsGenericType)
                        {
                            continue;
                        }
                        var baseType = propertyType.GetGenericTypeDefinition();
                        if (baseType != typeof(InArgument<>) && baseType != typeof(OutArgument<>))
                        {
                            continue;
                        }
                        // Get the generic type argument
                        var genericArgs = propertyType.GetGenericArguments();
                        if (genericArgs.Length != 1)
                        {
                            continue;
                        }
                        var argumentType = genericArgs[0];
                                    
                        // Check if the argument type is valid
                        if (!validTypes.Contains(argumentType))
                        {
                            var argumentKind = baseType == typeof(InArgument<>) ? "InArgument" : "OutArgument";
                            problems.Add($"Plugin {assemblyType.FullName} {argumentKind} {propertyInfo.Name} has invalid type {argumentType.Name}. Valid types are: bool, DateTime, Decimal, Double, EntityReference, int, Money, OptionSetValue, string");
                        }
                    }
                }
            }
            return problems;
        }
    }
}
