# CuttleText #

CuttleText is a super lightweight text processing engine, specifically designed to inject code segments into source code files (though, just like duct tape isn't only for ducts, CuttleText isn't only for CS files).

## How It Works ##

The easiest way to see what CuttleText is capable of is to learn how to use it (it's very simple).

CuttleText "hydrates" a template file using any number of "Injectors" that appear within the template.

Let's look at a typical template file:

    using System.Collections;
    
    public class MyClass
    {
    	// --> MyCodeInjector arg1 arg2 arg3
    }

The `// -->` is a special identifier, alerting CuttleText that you want to inject some code at this location.  The specific code you want to inject will be provided by an injector named "MyCodeInjector," which might look like this:

	class MyCodeInjector : InjectorBase
    {
        public override void Inject(List<string> cmdLine)
        {
			// cmdLine contains { "MyCodeInjector", "arg1", "arg2", "arg3" } 

			Write("if (something)");
			OpenBracket();
			for (int ndx = 0; ndx < 3; ++ndx) {
				Write("someClass.DoSomething(" + ndx + ");");
			}
			CloseBracket();
        }
    }

You'll end up with a final output file that looks like this:

    using System.Collections;
    
    public class MyClass
    {
    	if (something)
		{
			someClass.DoSomething(0);
			someClass.DoSomething(1);
			someClass.DoSomething(2);
		}
    }

Notice how the CuttleText processor automatically handles indents, and how the injectors can take "command line arguments" from the template file.

You can also perform simple token morphing (for example, make all floating point tokens end with f), as well as simple text substitution (replace "FOO" with "BAR" anywhere in the template).

## Using CuttleText ##

The process is straightforward:

1. Create any number of Injector classes (must derive from InjectorBase and implement Inject() method).

2. Instantiate a Hydrator, add the Injectors to it, then turn it loose on a template file:

> 	var hydrator = new Hydrator();
> 
>     hydrator.AddInjector(new MessageMethodInjector());
>     hydra.AddStringSubst("CLASS_NAME", "MyClass");
> 
> 	string templateStr = /* load your template CS file here */;
> 
>     outputStr = hydra.HydrateIntoString(templateStr); 

Your transformed text will be in `outputStr`.

## String Substitution ##

In the example above notice the `AddStringSubst` call.  That call tells the Hydrator to replace any occurrence of `CLASS_NAME` with `MyClass`.  

## Passing a context to your Injectors ##

You can create a context class (derived from `ContextBase`):

	class MyContext : ContextBase
	{
		public string[] SomeData;
	}

And provide an instance of it to the hydrator at construction time:

	var theContext = new MyContext();
	theContext.SomeData = /* whatever */;

	var hydrator = new Hydrator(theContext);

Then inside your Injectors, you can get the context originally supplied:

	class MyCodeInjector : InjectorBase
    {
        public override void Inject(List<string> cmdLine)
        {
			MyContext ctx = (MyContext)_context;
			/* do what you need to... */
		}
	}

Contexts also provide a logging abstract function - you can fill this in to route log statements coming from the injectors. 

## Writing an Injector ##

The only thing you really need to fill out is the `Inject()` method.  Inside it, you can use the following base class methods:

`Write(string line)` - Adds the given string to the output.  Newlines are automatically added. 

`Indent()` - Indents all subsequent calls to Write by 1 tab stop.

`Unindent()` - Unindents all subsequent calls to Write by 1 tab stop.  If there is no indent, does nothing.
 
`OpenBracket()` - writes `{` and calls `Indent()`.  `OpenBracket("// foo")` will write `{ // foo`. 

`CloseBracket()` -  writes `}` and calls `Unindent()`.  `CloseBracket("// foo")` will write `} // foo`.
 
In addition you can use the following methods to output log statements.  All of these just call the context-provided logging function.  If you haven't overridden the logging functions in your own context, none of these will do anything.
 
	FatalError(string message) 
	Error(string message)
	InternalError(string message)
	Warning(string message)
	Info(string message)

## Token Morphing ##

You can also supply token morpher classes to the Hydrator, similar to providing injectors - use the `AddTokenMorpher()` call.

Your token morphers must derive from `TokenMorpherBase`, and implement the `MorphToken` abstract.

After all text substitutions are done, CuttleText will call the token morphers, in the order they were registered, on each token of the generated file.  This allows you to "catch" tokens of a certain type (say, numbers) and inject characters as you see fit (for example, add "f").  

## Tada! ##

That's it!  CuttleText is a pretty straightforward little library... simple but surprisingly useful.  If you have suggestions or pull requests or whatever, hit me up!

