# Contribution Guidelines

## Table of Contents

 * [Branching Model](#branching-model)
 * [Pull Requests](#pull-requests)
 * [Coding Style](#coding-style)
	 * [Layout](#layout)
	 * [Comments](#comments)
	 * [Language](#language)
	 * [Arrays](#arrays)
	 * [Implicit Typing](#implicit-typing)
	 * [Using](#using)
	 * [Logical Operators](#logical-operators)
	 * [Implicit Typing and new](#implicit-typing)
	 * [Object Initialization](#object-initialization)
	 * [Event Registration](#event-registration)
	 * [Static Members](#static-members)
	 * [LINQ](#linq)
	 * [Misc.](#misq)

## Branching Model
 - Merges to `master` are releases
 - `develop` contains current version that is worked on
 - New code is developed in your own Fork and merged via Pull Request into `develop`
 - Branches are categorized into `Enhancement`,  `Feature`, and `Fix`
	 - You can of course suggest new Categories
 - Merges will be approved by @SuNflOw1991 and @CapCalamity only

## Pull Requests
 - when you're satisfied with your changes, open a [Pull Request](https://help.github.com/articles/using-pull-requests/)
 - rebase your changes on `develop` to keep your branch up-to-date
 - If you have already opened a PR and your changes are conflicting, merge them on your own
 - try to keep the branch as simple as possible to reach the desired outcome
 - similarly, keep commit small and simple
 - Write up a description of what you did, possibly also why
 - offer a guide how to test or review the changes you made

## Coding Style

We chose to follow Microsofts official C# coding guidlines, which can be found [here](https://msdn.microsoft.com/en-us/library/ff926074.aspx)

### Layout
 - Use smart indenting, four-character indents, tabs saved as spaces
 - Write only one statement per line
 - Write only one declaration per line
 - Indent continuation lines one tab stop
 - Add one blank line between logically different groups
 - Use parentheses to make clauses in an expression apparent

### Comments
 - Place the comment on a separate line, not at the end of a line of code.
 - Begin comment text with an uppercase letter.
 - Insert one space between the comment delimiter (//) and the comment text

### Language
 - Use the + operator to concatenate short strings
 - Use `string.Format` for more complex concatenations
 - To append strings in loops, especially when you are working with large amounts of text, use a `System.Text.StringBuilder` object
 - In general, use int rather than unsigned types
 - Use `Try-Catch-Finally` to handle Exceptions
 - 
### Arrays
 - Use the [concise syntax](https://msdn.microsoft.com/en-us/library/Bb384062.aspx) when you initialize arrays on the declaration line
```c#
// Preferred syntax. Note that you cannot use var here instead of string[].
string[] vowels1 = { "a", "e", "i", "o", "u" };

// If you use explicit instantiation, you can use var.
var vowels2 = new string[] { "a", "e", "i", "o", "u" };
```

### [Implicit Typing](https://msdn.microsoft.com/en-us/library/bb384061.aspx)
 - Use implicit typing for local variables when the type of the variable is obvious from the right side of the assignment
 - Do not use `var` when the type is not apparent from the right side of the assignment
 - Do not rely on the variable name to specify the type of the variable as it might not be correct
 - Don't use `dynamic` unless necessary
 - Use implicit typing to determine the type of the loop variable in `for` and `foreach` loops, unless it is unclear what type it will be

### [Using](https://msdn.microsoft.com/en-us/library/yh598w02.aspx)
 - Simplify your code by using the C# using statement
 - If you have a try-finally statement in which the only code in the finally block is a call to the Dispose method, use a using statement instead
```c#
Font font1 = new Font("Arial", 10.0f);
try
{
    byte charset = font1.GdiCharSet;
}
finally
{
    if (font1 != null)
    {
        ((IDisposable)font1).Dispose();
    }
}

// You can do the same thing with a using statement.
using (Font font2 = new Font("Arial", 10.0f))
{
    byte charset = font2.GdiCharSet;
}
```

### [Logical Operators](https://msdn.microsoft.com/en-us/library/ms173145.aspx)
 - Use `&&` and `||` in favor of `&` and `|`, unless your operation requires bitwise operations (Flags, Masks, ...)

### [Implicit Typing](https://msdn.microsoft.com/en-us/library/bb384061.aspx) and `new`
 - Use the concise form of object instantiation, with implicit typing
```c#
var instance1 = new ExampleClass();
```

### [Object Initialization](https://msdn.microsoft.com/en-us/library/Bb384062.aspx)
 - Use Object Initialization to simplify object creation
```c#
// Object initializer.
var instance3 = new ExampleClass { Name = "Desktop", ID = 37414, 
    Location = "Redmond", Age = 2.3 };

// Default constructor and assignment statements.
var instance4 = new ExampleClass();
instance4.Name = "Desktop";
instance4.ID = 37414;
instance4.Location = "Redmond";
instance4.Age = 2.3;
```

### [Event Registration](https://msdn.microsoft.com/en-us/library/ms366768.aspx) 
 - Use Lambda Expressions for events that do not need to be removed later on
 - Use Dedicated Functions with descriptive names otherwise
```c#
public Form2()
{
    // You can use a lambda expression to define an event handler.
    this.Click += (s, e) =>
    {
        // ...
    };
}

// Using a lambda expression shortens the following traditional definition.
public Form1()
{
    this.Click += new EventHandler(Form1_Click);
}

void Form1_Click(object sender, EventArgs e)
{
	// ...
}
```

### [Static Members](https://msdn.microsoft.com/en-us/library/79b3xss3.aspx)
 - Call static members by using the class name: ClassName.StaticMember. This practice makes code more readable by making static access clear. Do not qualify a static member defined in a base class with the name of a derived class. While that code compiles, the code readability is misleading, and the code may break in the future if you add a static member with the same name to the derived class

### [LINQ](https://msdn.microsoft.com/en-us/library/mt693042.aspx)
 - Use Method Syntax, unless Query Syntax offers a significant benefit

### Misc.
 - don't clutter commits with style-changes on code that doesn't belong to that commit, do that in dedicated commit or enhancements
