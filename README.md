# Comparser  
Complex Computer Parser  

-currently in development, some parts might not work properly yet-
  
EXECUTION STAGES:  
Comparser has two distinct stages:  
Parser stage (COMMANDS) — read sequentially. Definitions, constants, if, while, do, etc. are processed here. Names may be redefined during this stage.  
Evaluation stage (EXPRESSIONS) — after parsing is complete, expressions are pure. Calling an expression cannot modify definitions, constants, or functions.  
Case-insensitive - all uppercase letters are internally converted to lowercase.  
  
COMMANDS:  
Parser stage, the "code". This one is read sequentially line by line, and works more like your typical procedural/imperative code.  
You can and re-define functions and constants (so they can be mutated while this code is being read, but then stay at their final value for the evaluation stage)  
  
Function definition:  
(_\<expressionCacheSize\>_)functionName(_\<expressionArguments\>_) : _\<expressionDefinition\>_  
CacheSize is a limit number that will get assigned to that function, for how many different arguments it will remember its evaluation.  
CacheSize is optional. It will only get read if the definition begins with parentheses. The default cache size is 1 for any function where it is not specified.  
You define a function multiple times with different arguments. If an argument evaluates to some value, that definition will only get matched with calls with the same value  
Multiple definitions of a function are tested in definition order. The first matching pattern/condition is used.  
Each expression has its own cache. If you define a function with multiple argument patterns, each definition will have its separate cache, with separate evaluation memory.  
Example of a pattern-matching multi-defition:  
factorial(0) : 1; factorial(x) : xfactorial(x - 1)  
Example of cacheSize:  
(0)function(x, y) : x + y^x /* this will have cache disabled, and each evaluation will get computed again, even if it is called with the very same arguments immediately again.  
function()  
  
Ternary function definition:  
(_\<expressionCacheSize\>_)function(_\<expressionArguments\>_) : _\<expressionCondition\>_ ? _\<expressionTrueDefinition\>_ : _\<expressionFalseDefinition\>_  
Works like argument pattern matching, except the condition can be complex instead of matching an argument exactly  
Example: factorial(x) : x \<= 1 ? 1 : xfactorial(x - 1)  
This example also generates two definitions. The first one will have the condition, and the second one would assume the condition was false if it gets matched after it. 
Ternary definitons can be chained, but not nested.  
Valid example: f(x) : _\<conditionA\>_ ? _\<ifA\>_ : _\<conditionB\>_ ? _\<elseIfB\>_ : _\<else\>_  
Invalid nested example: f(x) : _\<conditionA\>_ ? _\<conditionB\>_ ? _\<ifAB\>_ : _\<ifAnotB\>_ : _\<ifnotA\>_   
Valid alternatives to that nested ternary:  
f(x) : _\<conditionA\>_ = 0 ? _\<ifNotA\>_ : _\<conditionB\>_ : _\<ifAB\>_ : _\<ifAnotB\>_   
Or:
f2(x) : _\<conditionB\>_ = 0 ? _\<ifAB\>_ :  _\<ifAnotB\>_   
f(x) : _\<conditionA\>_ = 0 ? f2(x) : _\<ifNotA\>_   
   
Default Argument Expressions:
  
f(x, y:2e^x, z:sin(y)) : 7z+5yz+2x
Arguments can get an expression that evaluates them. If there is no value supplied to an argument by not writing it at all, or giving "\_" (supplied values override it).  
The example above would only require 1 argument, which would set up y and z, and all of them will be used in the called function's body  
(Can use it even multiple times while computing  once, like a precomputed variable)  
The expressions can even reference the other neighboring arguments, but only those to the left (as those to the right were not read yet, when this is binding).  
So you can't do this: f(x:y+1,y)=x, but you can when you swap those arguments.  
Examples:  
f(x,y:2x+1):2^y; f(2) = 32; f(_, 4) = 16  
  
Separators:  
;  
\n  
Works very much like in other languages, but it is optional, as new lines also work like separators.  
  
Variable definition:  
variableName : _\<expressionValue\>_  
-They can be mutable when you write definitions with the same name multiple times. It will have the value that was defined the last time during reading.  
Example: x : 1; print : x, ","; x : 2; print=x; /* prints 1, 2  
  
Print:  
print : _\<expressionArgument\>_  
Takes all the elements in the evaluated vector from the expression and prints them into the log  
  
Do:  
do : _\<expressionArgument\>_  
Takes all the string-type elements in the evaluated vector from the expression, and puts them in from the program counter to be parsed like the following commands.  
Basically dynamically inserts dynamically generated code, as long as the syntax is valid.  
  
If:  
? _\<expressionCondition\>_ { _\<commands\>_ }  
Functions just like if in other languages, only the syntax is slightly different. The keyword "if" is just a question mark, and it doesn't require parentheses  
  
Else:  
: { _\<commands\>_ }  
Functions just like else in other languages. The syntax is different in the same way as if. Must follow immediately after the closing bracket of an if.  
Elses can be chained. For example:  
? _\<condition\>_ { _\<commandsA\>_ } : { _\<commandsB\>_ } : { _\<commandsC\>_ } : { _\<commandsD\>_ }  
Will have the block A and C run if the condition is true, and B and D if false. Else is entered is the block before it was skipped.  
I don't think this is particularly useful, but it's just how the parser works, so it's worth mentioning.  
  
While:  
! _\<expressionCondition\>_ { _\<commands\>_ }  
Functions just like while. The syntax is again different in the same way as if/else.  
Can have an else branch like if. It would get called only if the condition is not met even initially.  

------------------------------------------------------------------------------------------------------------------------------------------------------------
  
EXPRESSIONS:  
The evaluation stage. After the initial code was read, you can only call pure expressions without any of the non-expression syntax that was described above.  
Written in a simple functional/mathematical way. Evaluates into a nestable vector.  
It doesn't support any of the syntax from above. There are no do, if/else, while, print, function/variable definitions, or even ternary operators.  
Supports this syntax:  
  
Parentheses:  
(\<expression\>)  
Mathematical-like parentheses enforcing an order of operations. Without them, it uses the default order of operations of math (PEMDAS).  
  
Vectors:  
\<expression\>, \<expression\>, \<expression\>  
Each expression gets evaluated, and you get back a vector with each result in the same order  
You can nest vectors with parentheses to make more complex structures.  
Invariant: vectors containing exactly one element are always collapsed into their containing level. The evaluator does not preserve one-element vector nesting.  
For example: 1, (2, 3), 4, (5, (6, 7)), 8, (9), (((10), 11)) will get collapsed into: 1, (2, 3), 4, (5, (6, 7)), 8, 9, (10, 11).  
This is to keep the actual structure that matters, and for any size 1 nests that could appear to be cleared out.  
  
Unary operations:

\<expression\>!
Factorial
-\<expression\>
Negate
  
Binary operators:  
\<expression\> + \<expression\>  
Add  
\<expression\> - \<expression\>  
Subtract  
\<expression\> * \<expression\>  
Multiply  
\<expression\> / \<expression\>  
Divide  
\<expression\> ^ \<expression\>  
Power  
\<expression\>\<expression\>  
Operator-less multiply  
\<expression\> = \<expression\>  
1 if equal, 0 if not  
\<expression\> != \<expression\>  
0 if equal, 1 if not  
\<expression\> \< \<expression\>  
1 if less, 0 if not  
\<expression\> \<= \<expression\>  
1 if less or equal, 0 if not  
\<expression\> \> \<expression\>  
1 if more, 0 if not  
\<expression\> \>= \<expression\>  
1 if more or equal, 0 if not  

Logic is done numerically. Any number with a norm \< 1 is false.
Use true(\<expression\>) to convert the number into its boolean value of 0 or 1
(unary "sqrabs(x) >= 1" operator). Use * as AND, + as OR, true(a) != true(b) as XOR  
Operations are recursively broadcast over vectors.
When vectors have different shapes, the shorter dimension is cyclically reused...
...and the deeper layers will re-access the previous levels of the other operand.  
Scalar values therefore naturally broadcast into vectors. 
And vectors may contain vectors of arbitrary depth.  
Example: (1, 2, 3) + (4, 5) = (1 + 4, 2 + 5, 3 + 4)  
Example: ((1, 2), (3, 4, 5), 6, 13) + ((7, 8, 9), 10, (11, 12)) 
= ((1 + 7, 2 + 8, 1 + 9), (3 + 10, 4 + 10, 5 + 10), (6 + 11, 6 + 12), (13 + 7, 13 + 8, 13 + 9))  
  
  
Vector Extractor:  
\<expression\>[\<expression\>]  
Extracts terms from a vector using indices in: [expression].  
Example: (a, b, c, d, e)[2] : c  
Example: (0a, 1b, 2c, (30d, 31e), 5f)[3, 2, (5, 1, 3)] : (30d, 31e), 2c, (5, 1, (30d, 31e))  
  
Functions:  
min, max, clamp, exp, ln, log10, sin, cosh, re, im, frac, floor, round, sgn, abs, conj, sqrt, cub, gauss, softmax, gamma, zeta...  
(full list below)  
All elementary and component-wise operations and then some.  
Binary operations are chain-applied to the first and second element, then to the result and the third element, and so on.  
So a binary operation like Min can take the minimum for any size vector.  
They return the first element unchanged if it doesn't have a second operand.  
Unary operations are applied to each element in the nested vector individually.  
  
eval(\<expression\>)  
Attempts to parse and evaluate every Text in the input  
Evaluates strings as expressions. The strings are parsed using the expression parser, but cannot execute parser-stage commands.  
Example: eval("1+2", "e^(ipi)") = 3, -1  
  
count(\<expression\>)  
counts the number of elements in the vector  
Example: count(0,1,2):3  
  
cat(\<expression\>)  
Un-nests the vector and concatenates all the elements next to each other on the top-level of the result vector  
  
sum(\<expressionIndex\>,\<expressionFrom\>,\<expressionTo\>,\<expression\>)  
iterative sum: sum(\<index\>,from,to,expression(k\<index\>))  
Example: sum(0, 1, 4, k0) = 1 + 2 + 3 + 4 = 10  
Can also iterate backwards, unlike the same iterators in math.  
If you want them to return empty sums like backwards math sums, or negatives like backwards integrals, you can define that with a  ternary definition:  
DiodeSum(x, from, to, expression) : from > to ? 0 : sum(x,from,to,expression)  
IntegralLikeSum(x, from, to, expression) : from \> to ? -sum(x, from, to, expression) : sum(x, from, to, expression)  
There is also "prod" function that functions the same way as an iterated product  
and there is also "vec" function, which doesn't add or multiply the terms, but puts them all directly into a vector.  
  
Constants:  
e | pi | tau | gamma | one  
Get replaced by their associated mathematical constant value  
Some algebras have their extra constants, like i in Complex numbers and i, j, k in Quaternions.  
  
Variables:  
Work by simple value substitution just like constants, but you define their names and values  
  
Numbers:  
Decimal notation: 123.456  
Scientific notation could be ambiguously confused with multiplication by Euler's number, so just write it like: 1.234*10^2  
  
Strings:  
"string"  
Adding strings concatenates them.  
Subtracting removes occurrences of the operand  
  
Comments:  
/* begins a comment and lasts until the end of the line or */  
  
  
Core evaluation rules
-Expressions evaluate to values or vectors.  
-One-element vectors are always collapsed.  
-Operators recursively broadcast over vectors.  
-Mismatched vector dimensions are cyclically broadcast.  
-Unary functions recursively apply to vector elements.  
-Multi-argument binary functions reduce their arguments from left to right.  
-Function definitions are selected by ordered pattern matching and conditions.  
-Evaluation is pure; definitions cannot be modified during evaluation.  
-Parser-stage commands are unavailable during evaluation.  
-Values are generic and may represent real, complex, quaternion, or other supported numeric types.  
