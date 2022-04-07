# elastic-search-dev01
poc api project for using NEST and ElasticSearch - c# and .NetCore

# ElasticSearchNetCorePOC

This repo contains the source code to demonstrate how to integrate C# applications with elastic searcg using NEST and custom queries.


## Execute the sample

```sh

dotnet ElasticSearchTest.dll create -f divina_commedia.txt -h http://localhost:9300
> Index created in 30338ms with 14006 element.

dotnet ElasticSearchTest.dll search -i divinacommediatxt -h http://localhost:9300 -q "dante AND virgi*"
> Searching for $dante AND virgi*
> "Dante, perché Virgilio se ne vada,
> "Dante, perché Virgilio se ne vada,

```


Screen-shot of sample curl command in cmd-window :

![CMDScreenShot](./ElasticSearchNetCorePOC/images/curl-cmd-elk-sample01.png)

## Quick Start Examples

1. Create a class to define valid options, and to receive the parsed options.
2. Call ParseArguments with the args string array.

C# Quick Start:

```cs
using System;
using CommandLine;

namespace QuickStart
{
    class Program
    {
        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if (o.Verbose)
                       {
                           Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                           Console.WriteLine("Quick Start Example! App is in Verbose mode!");
                       }
                       else
                       {
                           Console.WriteLine($"Current Arguments: -v {o.Verbose}");
                           Console.WriteLine("Quick Start Example!");
                       }
                   });
        }
    }
}
```

## C# Examples:

<details>
  <summary>Click to expand!</summary>

```cs

class Options
{
  [Option('r', "read", Required = true, HelpText = "Input files to be processed.")]
  public IEnumerable<string> InputFiles { get; set; }

  // Omitting long name, defaults to name of property, ie "--verbose"
  [Option(
	Default = false,
	HelpText = "Prints all messages to standard output.")]
  public bool Verbose { get; set; }
  
  [Option("stdin",
	Default = false,
	HelpText = "Read from stdin")]
  public bool stdin { get; set; }

  [Value(0, MetaName = "offset", HelpText = "File offset.")]
  public long? Offset { get; set; }
}

static void Main(string[] args)
{
  CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed(RunOptions)
    .WithNotParsed(HandleParseError);
}
static void RunOptions(Options opts)
{
  //handle options
}
static void HandleParseError(IEnumerable<Error> errs)
{
  //handle errors
}

```

</details>
