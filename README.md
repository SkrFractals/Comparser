# Comparser
Complex Computer Parser

COMMANDS:

Function definition:
(<expressionCacheSize>)functionName(<expressionArguments>) = <expressionDefinition>
-cacheSize is a limit number that will get assigned to that function, for how many different arguments it will remember its evaluation.
-cacheSize is optional. It will only get read if the definition begins with parentheses
-you define a function multiple times with different arguments. If an argument evaluates to some value, that definition will only get matched with calls with the same value
-example of a pattern-matching multi-defition: factorial(0) = 1; factorial(x)=xfactorial(x-1)

Ternary function definition:
(<expressionCacheSize>)function(<expressionArguments>) = <expressionCondition> ? <expressionTrueDefinition> : <expressionFalseDefinition>
-works like argument pattern matching, except the condition can be complex instead of matching an argument exactly
-Example: factorial(x)=x<=1?1:xfactorial(x-1)
-This example also generates two definitions. The first one will have the condition, and the second one would assume the condition was false if it gets matched after it.

Separators:
; | \n
Works very much like in other languages, but it is optional, as new lines also work like separators.

Variable definition:
variableName=<expressionValue>
-they can be mutable when you write definitions with the same name multiple times. It will have the value that was defined the last time during reading.
Example: x=1; print = x, ","; x=2; print=x; /* prints 1,2

Print:
print=<expressionArgument>
Takes all the elements in the evaluated vector from the expression, and prints them into the log

Do:
do=<expressionArgument>
Takes all the string-type elements in the evaluated vector from the expression, and puts them in from the program counter to be parsed like the following commands.
Basically dynamically inserts dynamically generated code, as long as the syntax is valid.

If:
? <expressionCondition> { <commands> }
Functions just like if in other languages, only the syntax is slightly different. The keyword "if" is just a question mark, and it doesn't require parentheses

Else:
: { <commands> }
Functions just like else in other languages. The syntax is different in the same way as if. Must follow immediately after the closing bracket of an if.

While:
! <expressionCondition> { <commands> }
Functions just like while. The syntax is again different in the same way as if/else.
Can have an else branch like if. It would get called only if the condition is not met even initially.


EXPRESSIONS:
Written in a simple functional/mathematical way. Evaluates into a vector. Supports this syntax:

Parentheses:
(<expression>)
Mathematical-like parentheses enforcing an order of operations. Without it, it uses the default order of operations of math (PEMDAS).

Vectors:
<expression>,<expression>,<expression>
Each expression gets evaluated, and you get back a vector with each result in the same order
You can nest vectors with parentheses to make more complex structures.

Binary operators:
<expression> + <expression>
Add
<expression> - <expression>
Subtract
<expression> * <expression>
Multiply
<expression> / <expression>
Divide
<expression> ^ <expression>
Power
<expression><expression>
Operator-less multiply
<expression> = <expression>
1 if equal, 0 if not
<expression> != <expression>
0 if equal, 1 if not
<expression> < <expression>
1 if less, 0 if not
<expression> <= <expression>
1 if less or equal, 0 if not
<expression> > <expression>
1 if more, 0 if not
<expression> >= <expression>
1 if more or equal, 0 if not
Logic is done numerically. Any number with a norm < 1 is false. Use true(<expression>) to convert the number into its boolean value of 0 or 1 (unary ">= 1" operator). Use * as AND, + as OR, true(a) != true(b) as XOR
Works recursively on nested vectors, and if their structures/lengths do not match, the smaller one gets modularly cycled, and the deeper layers will re-access the previous levels of the other operand.
Example: (1,2,3) + (4,5) = (1+4,2+5,3+4)
Example: ((1,2),(3,4,5),6,13) + ((7,8,9),10,(11,12)) = ((1+7,2+8,1+9),(3+10,4+10,5+10),(6+11,6+12),(13+7,13+8,13+9))

Factorial:
<expresion>!

Vector Extractor:
<expression>[<expression>]
Extracts terms from a vector using indices in: [expression].
Example: (a,b,c,d,e)[2] = c
Example: (0a,1b,2c,(30d,31e),5f)[3,2,(5,1,3)] = (30d,31e),2c,(5,1,(30d,31e))

Functions:
min, max, clamp, exp, ln, log10, sin, cosh, re, im, frac, floor, round, sgn, abs, conj, sqrt, cub, gauss, softmax, gamma, zeta...
All elementary and component-wise operations and then some.
Binary operations will get chain-applied if there are more than 2 terms in the argument vector
Unary operations get applied to every term in the vector, recursively

eval(<expression>)
attempts to parse and evaluate every Text in the input
Example: eval("1+2","e^(ipi)")=3,-1

count(<expression>)
counts the number of elements in the vector
Example: count(0,1,2)=3

cat(<expression>)
Un-nests the vector and concatenates all the elements next to each other on the top-level of the result vector

sum(<expressionIndex>,<expressionFrom>,<expressionTo>,<expression>)
iterative sum: sum(<index>,from,to,expression(k<index>))
Example: sum(0,1,4,k0) = 1+2+3+4 = 10
There is also "prod" function that functions the same way as an iterated product
and there is also "vec" function, which doesn't add or multiply the terms, but puts them all directly into a vector.

Constants:
e | pi | tau | gamma | one
Get replaced by their associated mathematical constant value
Some algebras have their extra constants, like i in Complex numbers and i,j,k in Quaternions.

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
/* */
/* begins a comment and lasts until the end of line or */
