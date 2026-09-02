# Comparser  
Complex Computer Parser  

-currently in development, some parts might not work properly yet-
  
## EXECUTION STAGES:  
Comparser has two distinct stages:  
Parser stage (COMMANDS) — read sequentially. Definitions, constants, if, while, do, etc. are processed here. Names may be redefined during this stage.  
Evaluation stage (EXPRESSIONS) — after parsing is complete, expressions are pure. Calling an expression cannot modify definitions, constants, or functions.  
Case-sensitivity optional - if you choose insensitive, all uppercase letters are internally converted to lowercase.  
  
## COMMANDS:  
Parser stage, the "code". This one is read sequentially line by line, and works more like your typical procedural/imperative code.  
You can and re-define functions and constants (so they can be mutated while this code is being read, but then stay at their final value for the evaluation stage)  
  
### <img width="17" height="15" alt="image" src="https://github.com/user-attachments/assets/ffc897ab-2858-4e47-98ad-ac34348366c9" />Function definition:  
(_\<expressionCacheSize\>_)functionName(_\<expressionArguments\>_) : _\<expressionDefinition\>_  
They have a light blue color in the editor in dark mode.  
CacheSize is a limit that will get assigned to that function, for how many different arguments it will remember its evaluation.  
CacheSize is optional. It will only get read if the definition begins with parentheses.  
The default built-in function cache is 1 for most simple functions.  
The default custom function cache size is 0 for any function where it is not specified.  
When the cache size is 0, each evaluation will get computed again, even if it is called with the very same arguments immediately again.  
It is recommended to be careful about using cache if you are mutating variables.  
If you call a cached function with the same arguments it has remembered, but internally there would be a mutated variable changing the result, it will return the old, unmutated result.  
You define a function multiple times with different arguments. If an argument evaluates to some value, that definition will only get matched with calls with the same value.  
Multiple definitions of a function are tested in definition order. The first matching pattern/condition is used.  
Each expression has its own cache. If you define a function with multiple argument patterns, each definition will have its separate cache, with separate evaluation memory.  
Example of a pattern-matching multi-defition:  
factorial(0) : 1; factorial(x) : xfactorial(x - 1)  
Example of cacheSize:  
(13)function(x, y) : x + y^x _/* this will have cache enabled to remember 13 input-result pairs.  
  
### Ternary function definition:  
(_\<expressionCacheSize\>_)function(_\<expressionArguments\>_) : _\<expressionCondition\>_ ? _\<expressionTrueDefinition\>_ : _\<expressionFalseDefinition\>_  
Works like argument pattern matching, except the condition can be complex instead of matching an argument exactly  
Example: factorial(x) : x \<= 1 ? 1 : xfactorial(x - 1)  
This example also generates two definitions. The first one will have the condition, and the second one would assume the condition was false if it gets matched after it. 
Ternary definitions can be chained, but not nested.  
Valid example: f(x) : _\<conditionA\>_ ? _\<ifA\>_ : _\<conditionB\>_ ? _\<elseIfB\>_ : _\<else\>_  
Invalid nested example: f(x) : _\<conditionA\>_ ? _\<conditionB\>_ ? _\<ifAB\>_ : _\<ifAnotB\>_ : _\<ifnotA\>_   
Valid alternatives to that nested ternary:  
f(x) : _\<conditionA\>_ = 0 ? _\<ifNotA\>_ : _\<conditionB\>_ : _\<ifAB\>_ : _\<ifAnotB\>_   
Or:
f2(x) : _\<conditionB\>_ = 0 ? _\<ifAB\>_ :  _\<ifAnotB\>_   
f(x) : _\<conditionA\>_ = 0 ? f2(x) : _\<ifNotA\>_   
   
### Default Argument Expressions:
f(x, y:2e^x, z:sin(y)) : 7z+5yz+2x
Arguments can get an expression that evaluates them. If there is no value supplied to an argument by not writing it at all, or giving "\_" (supplied values override it).  
The example above would only require 1 argument, which would set up y and z, and all of them will be used in the called function's body  
(Can use it even multiple times while computing  once, like a precomputed variable, since default argument expressions are initiated with cache 1)  
The expressions can even reference the other neighboring arguments, but only those to the left (as those to the right were not read yet, when this is binding).  
So you can't do this: f(x:y+1,y)=x, but you can when you swap those arguments.  
Examples:  
f(x,y:2x+1):2^y; f(2) = 32; f(_, 4) = 16  
  
### Separators:  
;  
\n  
Works very much like in other languages, but it is optional, as new lines also work like separators.  
Semicolon is the most reliable separator, and will work even in a single line.
New lines can work as separators as long as it is unambiguous.
For example, if your old line is ending with an operator or opening parenthesis, bracket, or if the next line continues with an operator...
...anything that obviously means an expression continues to the next line, the newline will not act as a separator, and it will be interpreted as a multi-line expression.
  
### <img width="17" height="17" alt="image" src="https://github.com/user-attachments/assets/8eaa1e9d-da83-45a1-98a4-02e99229b6df" />Variable definition:  
variableName : _\<expressionValue\>_  
They have a bright green color in the editor in dark mode.  
-They can be mutable when you write definitions with the same name multiple times. It will have the value that was defined the last time during reading.  
Example: x : 1; print : x, ","; x : 2; print=x; _/* prints 1, 2_  
  
  
## <img width="14" height="15" alt="image" src="https://github.com/user-attachments/assets/3f4d3c96-8a4d-4561-9874-f349861a45f4" />ACTIONS:  
_\<actionName\>_ : _\<expressionArgument\>_  
Same syntax as defining constants, but using the core set of keywords instead of your custom names.  
They have a bright golden color in the editor in dark mode.  
  
### If:  
if : _\<expressionCondition\>_ { _\<commands\>_ }  
Functions just like if in other languages, only the syntax is slightly different. The keyword "if" is just a question mark, and it doesn't require parentheses  
Conditions like in these ifs, elses, and whiles are expressions, where the first leaf value will be taken, and it's size compared to 1. Less than 1 is false.
  
### Else:  
: _\<expressionCondition\>_ { _\<commands\>_ }  
Functions just like elseif in other languages. The syntax is different in the same way as if. Must follow immediately after the closing bracket of an if.  
You can chain more of them, like normally with else ifs. And they can also be chained after a while (get entered if the while failed at the beginning)
  
### While:  
while : _\<expressionCondition\>_ { _\<commands\>_ }  
Functions just like while. The syntax is again different in the same way as if/else.  
Can have an else branch like if. It would get called only if the condition is not met even initially.  
It can trigger a loop limit overflow if the condition is true too many times.  

### Return:  
Breaks out of the specified number of blocks, treating all blocks the same, whether they are ifs or whiles.  
Example: if:1{if:1{while:1{return:2;printvalue:1}printvalue:2}printvalue:3}printvalue:4 /* Prints 3\n4  
  
### Break:  
Breaks out of the specified number of loop blocks, very similar to return, but it doesn't count if block endings.  
Example: if:1{while:1{while:1{if:1{break:1;printvalue:1}printvalue:2}printvalue:3}printvalue:4}printvalue:5 /* Prints 3\n4\n5  
  
### Continue:  
Breaks out of the specified number of loop blocks, and returns back to the last one to retest it.  
Very similar to break, except it does that return back at the last break out, so it is not a break out on that one anymore.  
Example: a:0;if:1{while:a<2{a:a+1;printvalue:a;while:1{if:1{continue:2;printvalue:10}printvalue:20}printvalue:30}printvalue:40}printvalue:50 /* Prints 1\n2\n20\n30
  
### Print:  
Takes all the elements in the evaluated vector from the expression and prints them into the log as expression equations.  
Example: f(0) : 1; f(x) : xf(x-1); print : f(5); /* Prints f(5) = 120  
  
### PrintValue:  
Takes all the elements in the evaluated vector from the expression and prints them into the log as pure values.  
It will trigger a Bad Expression error if the value is NaN.  
Example: f(0) : 1; f(x) : xf(x-1); printvalue : f(5); /* Prints 120  
  
### Do:  
do : _\<expressionArgument\>_  
Takes all the string-type elements in the evaluated vector from the expression, and puts them in from the program counter to be parsed like the following commands.  
Basically dynamically inserts dynamically generated code, as long as the syntax is valid.  
It can trigger a stack overflow if it unpacks too many strings recursively.  
  
  
------------------------------------------------------------------------------------------------------------------------------------------------------------
  
## EXPRESSIONS:  
The evaluation stage. After the initial code was read, you can only call pure expressions without any of the non-expression syntax that was described above.  
Written in a simple functional/mathematical way. Evaluates into a nestable vector.  
It doesn't support any of the syntax from above. There are no do, if/else, while, print, function/variable definitions, or even ternary operators.  
Supports this syntax:  
  
### Parentheses:  
(\<expression\>)  
Mathematical-like parentheses enforcing an order of operations. Without them, it uses the default order of operations of math (PEMDAS).  
  
### Vectors:  
\<expression\>, \<expression\>, \<expression\>  
Each expression gets evaluated, and you get back a vector with each result in the same order  
You can nest vectors with parentheses to make more complex structures.  
Invariant: vectors containing exactly one element are always collapsed into their containing level. The evaluator does not preserve one-element vector nesting.  
For example: 1, (2, 3), 4, (5, (6, 7)), 8, (9), (((10), 11)) will get collapsed into: 1, (2, 3), 4, (5, (6, 7)), 8, 9, (10, 11).  
This is to keep the actual structure that matters, and for any size 1 nests that could appear to be cleared out.  
   
### Unary operations:  
  
_\<expression\>_!  
Factorial  
-_\<expression\>_  
Negate  
/_\<expression\>_  
Inverse  
\\_\<expression\>_  
Left inverse  
_\<expression\>_&  
Square  
_\<expression\>_~  
Conjugate  
_\<expression\>_@  
Absolute value, norm  
_\<expression\>_ @@  
Squared norm. Square of the absolute value.  
_\<expression\>_|  
Component-wise absolute value   
Example (5+i)@@ = 26  
_\<expression\>_#  
Count top-level vector elements  
Example: (1,2,3,(4,5,6))# = 4  
  
### Binary operators:  
_\<expression\>_ + _\<expression\>_  
Add  
(if any operand is a string, it will add the other operand as a string)  
_\<expression\>_ - _\<expression\>_  
Subtract  
__\<expression\>_ * _\<expression\>_  
Multiply  
If the LEFT operand is a string, it will try to call a function matching that string's name on the right operand(s).  
_\<expression\>_ / _\<expression\>_  
Divide  
_\<expression\>_ \ _\<expression\>_  
Divide from left (only differs in non-commutative algebras like quaternions)  
_\<expression\>_ % _\<expression\>_  
Complex division remainder  
_\<expression\>_ %% _\<expression\>_  
Component-wise remainder  
_\<expression\>_ ^ _\<expression\>_  
Power  
_\<expression\>_ $ _\<expression\>_  
The root of a power  
Example 1000 $$ 3 = 10  
_\<expression\>_ $$ _\<expression\>_  
Logarithm of operand's base.  
Example 1000 $$ 10 = 3  
_\<expression\>_  _\<expression\>_  
Operator-less multiply  
_\<expression\>_ = _\<expression\>_  
1 if equal, 0 if not  
_\<expression\>_ != _\<expression\>_  
0 if equal, 1 if not  
_\<expression\>_ \\ _<\<expression\>_  
1 if less, 0 if not  
_\<expression\>_ \<= _\<expression\>_  
1 if less or equal, 0 if not  
_\<expression\>_ \> _\<expression\>_  
1 if more, 0 if not  
_\<expression\>_ \>= _\<expression\>_  
1 if more or equal, 0 if not  
Count all of the nested vector elements  
Example: (1,2,3,(4,5,6))## = 6  
  
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
  
  
### Vector Extractor:  
_\<expression\>_[_\<expression\>_]  
Extracts terms from a vector using indices in: [expression].  
Example: (a, b, c, d, e)[2] : c  
Example: (0a, 1b, 2c, (30d, 31e), 5f)[3, 2, (5, 1, 3)] : (30d, 31e), 2c, (5, 1, (30d, 31e))  
  
### <img width="16" height="16" alt="image" src="https://github.com/user-attachments/assets/8157451f-687a-4c7e-b073-6a1cd949ea0b" />Default Functions:  
min, max, clamp, exp, ln, log10, sin, cosh, re, im, frac, floor, round, sgn, abs, conj, sqrt, cub, gauss, softmax, gamma, zeta...  
(full list below)  
They have a light violet color in the editor in dark mode.  
All elementary and component-wise operations and then some.  
Binary operations are chain-applied to the first and second element, then to the result and the third element, and so on.  
So a binary operation like Min can take the minimum for any size vector.  
They return the first element unchanged if it doesn't have a second operand.  
Unary operations are applied to each element in the nested vector individually.  
  
### eval(_\<expression\>_)  
Attempts to parse and evaluate every Text in the input  
Evaluates strings as expressions. The strings are parsed using the expression parser, but cannot execute parser-stage commands.  
Example: eval("1+2", "e^(ipi)") = 3, -1  
  
### count(_\<expression\>_)  
counts the number of elements in the vector  
Example: count(0,1,2):3  
  
### cat(_\<expression\>_)  
Un-nests the vector and concatenates all the elements next to each other on the top-level of the result vector  
  
### sum(_\<stringIndex\>_,_\<expressionFrom\>_,_\<expressionTo\>_,_\<stringExpression\>_)  
iterative sum: sum(_\<index\>_,from,to,expression(k_\<index\>_))  
Example: sum("k", 1, 4, "2k") = 2 + 4 + 6 + 8 = 20  
Can also iterate backwards, unlike the same iterators in math.  
If you want them to return empty sums like backwards math sums, or negatives like backwards integrals, you can define that with a  ternary definition:  
DiodeSum(x, from, to, expression) : from > to ? 0 : sum(x,from,to,expression)  
IntegralLikeSum(x, from, to, expression) : from \> to ? -sum(x, from, to, expression) : sum(x, from, to, expression)  
There is also "prod" function that functions the same way as an iterated product  
and there is also "vec" function, which doesn't add or multiply the terms, but puts them all directly into a vector.  
  
### <img width="16" height="16" alt="image" src="https://github.com/user-attachments/assets/72aed23b-74c1-4b8f-b5e2-6187c28bc611" />Arguments:  
They have an orange color in the editor in dark mode.  
Any xpression call get these as an input.  When you define a function, you can give it any number of arguments and name them.
Or give then a default expression that would evaluate them if the caller didn't supply any.
And if you just write something that successfult evaluates into a number instead of a name, the function will pattern match whether the caller provides that value. 
  
### <img width="16" height="16" alt="image" src="https://github.com/user-attachments/assets/2ce7d181-75b4-45da-835e-b456ec0891e3" />Constants:  
e | pi | tau | gamma | one  
They have a dark green color in the editor in dark mode.  
Get replaced by their associated mathematical constant value  
Some algebras have their extra constants, like i in Complex numbers and i, j, k in Quaternions.  
  
### <img width="17" height="17" alt="image" src="https://github.com/user-attachments/assets/bedcbf08-0adb-45a9-befe-4d4e33e16e45" />Variables:  
Work by simple value substitution just like constants, but you define their names and values.  
They have a light green color in the editor in dark mode.  
  
### <img width="18" height="16" alt="image" src="https://github.com/user-attachments/assets/5ebf7dbf-9c7e-4fd6-9f38-eba6690be74d" />Numbers:  
They have a dark purple golden color in the editor in dark mode.  
Decimal notation: 123.456  
Scientific notation could be ambiguously confused with multiplication by Euler's number, so just write it like: 1.234*10^2  
  
### NaN:  
\_  
"Not a number" is a result of an undefined operation, or a wrongly parsed expression.  
It can also be used as a discard argument, which would prompt its default expression if it has any.  
  
### <img width="16" height="16" alt="image" src="https://github.com/user-attachments/assets/289f0145-63be-4cff-8b92-ebea6bcd85ec" />Strings:  
"string"  
They have a pink color in the editor in dark mode.  
Adding strings concatenates them.  
Subtracting removes occurrences of the operand.  
Multiplying a string with a value on the right acts as a dynamic delegate call of a function whose name matches that string.  
  
### <img width="17" height="16" alt="image" src="https://github.com/user-attachments/assets/7b4dc667-ce86-4291-9167-2a791c416705" />Delegate Calls:  
They have a dark violet color in the editor in dark mode.  
There are two way this can be done.  
1. Constant: _\<constantString\>_(_\<expressionArgument\>_)
If you have a string in a constant, that matches any function name (including your defined functions), you can call that constant like that function.
You must write parentheses the same way as calling a regular function.  
Example: a:"sin";print:a(pi/2); /* prints a(pi/2) = 1  
3. Dynamic: _\<variableString\>\<expressionArgument\>_  
Multiplying a string that matches any default or user function with any value on its right will virtually call that function on that value.
You can write parentheses for visual resemblance, but it's really an overload of multiplication for strings, so it can also use operator-less multiply.
Example: twoXsevenY(x,y):2x+7y;a:"twoXsevenY";b(d,x):dx;print:b(a,1,2); /* prints b(a,1,2) = 16  
  
### <img width="15" height="16" alt="image" src="https://github.com/user-attachments/assets/ccd3a95b-4097-4d64-9cf3-387d063e62af" />Comments:  
/* comments are in between these, and are completely ignored by the parser's logic, unless they are inside strings. */  
They have a dark grey color in the editor in dark mode. (and light gray in light mode)  
They can be anywhere except inside words and operators. You can even use them to continue to a new line in the few places that don't normally allow that.
  
### <img width="16" height="14" alt="image" src="https://github.com/user-attachments/assets/3f593571-8493-41ca-91b3-cec1521d72f2" />Errors:  
They have a bright red color in the editor in dark mode.  
1. Stack Overflow - if the expression evaluation thinks it caught itself in an infinite loop (the default limit is 499, and can be adjusted in the Comparser constructor)  
Examples of stack overflow:  
f(x)=f(x) /* and call f(anything)  
f(x,y:y):y /* and call f with a single argument (a second argument would override the infinite loop with a supplied value). Also, the expression must contain y, so it asks the argument y to be evaluated.  
There could be other ways, like with eval, etc.  
  
  
## Core evaluation rules
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
  
------------------------------------------------------------------------------------------------------------------------------------------------------------  
  
## <img width="16" height="16" alt="image" src="https://github.com/user-attachments/assets/8157451f-687a-4c7e-b073-6a1cd949ea0b" />Full default function list:  

### Vector/Meta functions:
eval(_\<string\>_) ...parses a string as an expression and evaluates it (might not work properly yet). Example: eval("1+1") = 2  
count(_\<vector\>_) ...counts the number of vector elements in the top layer. Example: count((1,2,3,4),5,6) = 3  
cat/concat(_\<vector\>_) ...unpacks the nesting of the vector, puts all the elements to one top layer. Example: cat((1,2,(3,4)),5,6) = 1,2,3,4,5,6  
  
### Binary operations (chainable/nestable):
min/minimum(_\<vector\>_,_\<vector\>_,...) ...component-wise minimum.  
max/maximum(_\<vector\>_,_\<vector\>_,...) ...component-wise maximum.  
softmin(_\<vector\>_,_\<vector\>_,...) ...component-wise soft minimum. Equals to  ln(e^a+e^b)  
softmax(_\<vector\>_,_\<vector\>_,...) ...component-wise soft maximum.  Equals to  -ln(e^(-a)+e^(-b))  
add(_\<vector\>_,_\<vector\>_,...) ...adds all elements together.  
mul/multiply(_\<vector\>_,_\<vector\>_,...) ...multiplies all elements together  
imcoef(_\<value\>_,_\<coef\>_) ...attempts to return the selected imaginary coefficient. Also equal to re(-\<value\>\<coef\>).  
cmod/compmod(_\<vector\>_) ...component-wise modulo (division remainder). If the divisor's component is 0, it will return 0 in that component. Example: cmod(5+10i,4+8i)=1+2i.  
  
### Ternary operations:  
clamp(_\<vector\>_,_\<min\>_,_\<max\>_) ...component-wise clamp. Also equal to min(\<max\>,max(\<min\>,\<vector\>).  

### Iterators:  
vec/vector(_\<variable\>_,_\<from\>_,_\<to\>_, _\<expression\>_)  
...evaluates the expression with an extra argument with the "variable" name from "from" to "to" (can go bidirectionally), and builds a vector from these evaluated results.  
sum(_\<variable\>_,_\<from\>_,_\<to\>_, _\<expression\>_)  
...evaluates the expression with an extra argument with the "variable" name from "from" to "to" (can go bidirectionally), and adds them all together. Also equal to add(vec(...)).   
prod/product(_\<variable\>_,_\<from\>_,_\<to\>_, _\<expression\>_)  
...evaluates the expression with an extra argument with the "variable" name from "from" to "to" (can go bidirectionally), and multiplies them all together. Also equal to mul(vec(...)).   
  
### Unary operations (evaluate each nested element individually, and returns in a vector with the same structure):  
sofabs(_\<vector\>_) ...Also equals softmax(0,_\<vector\>_), or ln(1+e^_\<vector\>_)  
softneg(_\<vector\>_) ...Also equals softmin(0,_\<vector\>_), or -ln(1+e^(-_\<vector\>_))  
true(_\<vector\>_) ...Turns magnitude under 1 into 0, and over 1 into 1. Example: true((.5,1.5),-1,1) = (0,1),1  
false(_\<vector\>_) ...Turns magnitude under 1 into 1, and over 1 into 0. Example: true((.5,1.5),-1,1) = (1,0),0  
exp(_\<vector\>_) ...Also equal to e^_\<vector\>_  
exp2(_\<vector\>_) ...Also equal to 2^_\<vector\>_  
exp10(_\<vector\>_) ...Also equal to 10^_\<vector\>_  
ln/log(_\<vector\>_) ...Natural logarithm    
log2(_\<vector\>_) ... Binary logarithm. Also equal to ln(_\<vector\>_)/ln(2)  
log10(_\<vector\>_) ...Decimal logarithm. Also equal to ln(_\<vector\>_)/ln(10)  
re/real(_\<vector\>_) ...returns the value with all imaginary parts zeroed.  
im/imagl(_\<vector\>_) ...returns sum of all imaginary coefficients.  
immg(_\<vector\>_) ...imaginary magnitude immg(r+ai+bj+ck) = sqrt(a^2+b^2+c^2)  
frac(_\<vector\>_) ...fractional part. Also equals _\<vector\>_-trunc(_\<vector\>_)  
trunc(_\<vector\>_) ...truncate  
floor(_\<vector\>_) ...round down  
round(_\<vector\>_) ...round  
ceil/ceiling(_\<vector\>_) ...round up  
sgn/sign(_\<vector\>_) ..._\<vector\>_/abs(_\<vector\>_)  
neg/negative(_\<vector\>_) ...-_\<vector\>_  
inv/inverse(_\<vector\>_) ...1/_\<vector\>_  
cabs/compabs/absri(_\<vector\>_) ...component-wise absolute value (for example the difference between mandelbrot and burning ship)  
sqrabs(_\<vector\>_) ...Square of the absolute value. Aka sqr(abs(_\<vector\>_)), or _\<vector\>_conj(_\<vector\>_)  
norm/abs/absolute(_\<vector\>_) ...Absolute value (length of the complex number in the plane). Also square root of the sqrabs.  
arg(_\<vector\>_) ...Argument. The radian angle of the value in the complex plane. Example arg(-1) = pi  
conj/conjugate(_\<vector\>_) ...negates all the imaginary coefficients in the number.  
sqrt(_\<vector\>_) ...Fast square root. Also _\<vector\>_^.5, but this should be slightly faster.  
sqr(_\<vector\>_) ...Fast square. Also _\<vector\>_*_\<vector\>_, but this should be slightly faster.  
cbrt(_\<vector\>_) ...Cube root.  
cub/cube(_\<vector\>_) ...Fast square. Also _\<vector\>_*_\<vector\>_, but this should be slightly faster.  
quart(_\<vector\>_) ...Fast hybercube. Also _\<vector\>_*_\<vector\>_*_\<vector\>_*_\<vector\>_, or sqr(sqr(*_\<vector\>_)), but this should be slightly faster.   
fact/factorial(_\<vector\>_) ...Complex factorial.  
gauss(_\<vector\>_) ...Gauss function. Also equals to e^(-sqr(_\<vector\>_))  
zeta(_\<vector\>_) ...Riemann Zeta function.  
cosh,sinh,tanh,coth,sech,csch(_\<vector\>_) ...All the hyberbolic functions.  
cos,sin,tan,cot,sec,csc(_\<vector\>_) ...All the trigonometric functions.  
acosh,asinh,atanh,acoth,asech,acsch(_\<vector\>_) ...All the inverse hyberbolic functions.  
acos,asin,atan,acot,asec,acsc(_\<vector\>_) ...All the inverse trigonometric functions.  
sinc(_\<vector\>_) ...Also equals to sin(_\<vector\>_)/_\<vector\>_.  
nsinc/sincpi(_\<vector\>_) ...Also equals to sinc(pi*_\<vector\>_).  
sinch/sinhc(_\<vector\>_) ...Also equals to sinh(_\<vector\>_)/_\<vector\>_.  
nsinch/nsinhc/sinchpi/sinhcpi(_\<vector\>_) ...Also equals to sinhc(pi*_\<vector\>_).  
cosc(_\<vector\>_) ...Also equals to (1-cos(_\<vector\>_))/_\<vector\>_.  
ncosc/coscpi(_\<vector\>_) ...Also equals to cosc(pi*_\<vector\>_).  
coshc/cosch(_\<vector\>_) ...Also equals to (1-cosh(_\<vector\>_))/_\<vector\>_.  
ncoshc/ncosch/coshcpi/coschpi(_\<vector\>_) ...Also equals to cosh(pi*_\<vector\>_).  
  
  
## PLOTTER:  
The app comes with a plotter component. It lets you override a function plot(z,t), where z is the plot input, and t is animation time.  
It will render a 1D or 2D plot of that function.  
You can choose any coordinate bases you want.  
And you can also override the coloring function like: rgb(z):hsvtorgb(loghsv(z))  
This feature is still in development, but it's almost finished.  


## GPU ACCELERATION:  
I'm also preparing a GPU shader that will be able to evaluate custom expressions called on the build.  
This still has a while to be finished, but I've already prepared a translation of the build into the bytecode from the upcoming GPU shader, and a CPU virtual machine for simulating it.  
Neither has been tested yet, though.  
It will not support any features that work with strings:  
No EVAL function, no string data, no dynamic delegates (constant delegates are fine - those are resolved during parsing).  
  
