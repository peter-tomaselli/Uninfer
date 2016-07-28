# Uninfer

Uninfer is a Visual Studio 2015 C# Analyzer and Code Fix provider based mainly on [this excellent tutorial](http://www.productiverage.com/creating-a-c-sharp-roslyn-analyser-for-beginners-by-a-beginner).

However, Uninfer's mission is different: to eliminate usage of the `var` keyword in C# code.

For every `var` it sees, Uninfer will emit an `Info`-level diagnostic:
* ```UIF001 Info: usage of 'var'. Type should be 'string[]'.```

Uninfer also includes a Code Fix provider that will replace any usage of `var` with the proper type automatically, in bulk if necessary. If you're unfamiliar, these fix-its appear in a very cheerful 'lightbulb' menu alongside the code in question.

### Eliminate `var` from _your_ C# code *today*!
