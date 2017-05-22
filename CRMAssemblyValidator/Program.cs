using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Activities;
using System.Reflection;
using Microsoft.Xrm.Sdk.Workflow;

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
                    foreach (Type assemblyType in assembly.ExportedTypes)
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
    }
}
