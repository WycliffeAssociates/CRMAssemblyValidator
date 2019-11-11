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
            Type[] allowedArgumentTypes =
            {
                typeof(string),
                typeof(int),
                typeof(EntityReference),
                typeof(double),
                typeof(float),
                typeof(bool),
                typeof(Money),
                typeof(DateTime),
            };
            Type inArgumentType = typeof(InArgument);
            Type outArgumentType = typeof(OutArgument);
            Type inputAttributeType = typeof(InputAttribute);
            Type outputAttributeType = typeof(OutputAttribute);
            List<string> problems = new List<string>();
            foreach (Type assemblyType in input.ExportedTypes)
            {
                if (assemblyType.BaseType == typeof(CodeActivity))
                {
                    foreach (PropertyInfo propertyInfo in assemblyType.GetProperties())
                    {
                        if (propertyInfo.PropertyType.BaseType == inArgumentType && !propertyInfo.GetCustomAttributes(inputAttributeType).Any())
                        {
                            problems.Add($"Plugin {assemblyType.FullName} InArgument {propertyInfo.Name} is missing an Input attribute");
                        }
                        else if (propertyInfo.PropertyType.BaseType == outArgumentType && !propertyInfo.GetCustomAttributes(outputAttributeType).Any())
                        {
                            problems.Add($"Plugin {assemblyType.FullName} OutArgument {propertyInfo.Name} is missing an Output attribute");
                        }

                        if (propertyInfo.PropertyType.BaseType == inArgumentType || propertyInfo.PropertyType.BaseType == outArgumentType)
                        {
                            if (!allowedArgumentTypes.Contains(propertyInfo.PropertyType.GenericTypeArguments[0]))
                            {
                                problems.Add($"Plugin {assemblyType.FullName} Argument {propertyInfo.Name} has an unsupported type of {propertyInfo.PropertyType.GenericTypeArguments[0].Name}");
                            }
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
    }
}
