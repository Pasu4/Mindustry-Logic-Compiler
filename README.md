# Mindustry Logic Compiler
Mindustry Logic Compiler is a compiler for High-Level Mindustry Logic (HLMlog), a high-level programming language that compiles to Mindustry Logic. It tries to make Mindustry Logic easier to write.
## Syntax
### Comments
A comment is opened by `//` and closed by a semicolon. Compiler comments are opened with `///` and don't show up in the compiled code.
```
// This is a comment;
//
This is a
multiline
comment
;
/// This comment does not show up in the code;
end;
```
compiles to
```
# This is a comment
# 
# This is a
# multiline
# comment
end
```
### jump / label keyword
The jump keyword jumps to a specified label. A condition can optionally be specified. If a label is at the end of the program, an `end` statement is appended.
```
i = 0;
// Jump without condition;
jump lbl1;
i = 1;
label lbl1;
// Jump with condition;
jump lbl2 if i == 1;
i = 2;
label lbl2;
// Appended end statement;
```
compiles to
```
set i 0
# Jump without condition
jump 3 always
set i 1
# Jump with condition
jump 5 equal i 1
set i 2
# Appended end statement
end
```
### if statement
An if statement checks a condition and executes its body if the condition is true. If the scope ends at the end of the program, it appends an `end` statement.
```
i = Read(cell1, 0);
if(i == 0)
{
    cell1.Write(1, 0);
}
```
compiles to
```
read i cell1 0
jump 3 notEqual i 0
write 1 cell1 0
end
```
### while loop
A while loop executes its scope until the exit condition is met, after which it exits. It is a post-test loop. To make it a pre-test loop, put it in an if statement.
```
i = 10;
while(i > 0)
{
    i = i - 1;
}
```
compiles to
```
set i 10
op sub i i 1
jump 1 greaterThan i 0
```
### for loop
A for loop increments a variable every time it runs and exits once it reaches an exit value. The loop starts at the first number
and exits if the second number is reached after incrementing, meaning the second number is exclusive.
```
j = 0;
for(i, 0, 10)
{
    j += i;
}
```
compiles to
```
set j 0
set i 0
op add j j i
jump 2 lessThan i 10
```
For loops are post-test loops.
### Operators
Operators are used to change the value of variables. The syntax is varies for each operator, you can find it in the table below. HLMlog also uses slightly different operators from Mlog:
| Operator                    | Mlog (Editor) | Mlog (Export) | HLMlog | Syntax                  |
|:---------------------------:|:-------------:|:-------------:|:------:|:-----------------------:|
| Addition                    | \+            | add           | \+     | `result = a + b;`       |
| Subtraction                 | \-            | sub           | \-     | `result = a - b;`       |
| Multiplication              | \*            | mul           | \*     | `result = a * b;`       |
| Division                    | /             | div           | /      | `result = a / b;`       |
| Integer division            | //            | idiv          | //     | `result = a // b;`      |
| Modulo                      | %             | mod           | %      | `result = a % b;`       |
| Exponentiation              | ^             | pow           | ^      | `result = a ^ b;`       |
| Equal                       | ==            | equal         | ==     | `result = a == b;`      |
| Not equal                   | not           | notEqual      |!=      | `result = a != b;`      |
| Logical AND                 | and           | land          | &&     | `result = a && b;`      |
| Less than                   | <             | lessThan      | <      | `result = a < b;`       |
| Less than or equal          | <=            | lessThanEq    | <=     | `result = a <= b;`      |
| Greater than                | \>            | greaterThan   | \>     | `result = a > b;`       |
| Greater than or equal       | \>=           | greaterThanEq | \>=    | `result = a >= b;`      |
| Strict equality             | ===           | strictEqual   | ===    | `result = a === b;`     |
| Bit-shift left              | <<            | shl           | <<     | `result = a << b;`      |
| Bit-shift right             | \>\>          | shr           | \>\>   | `result = a >> b;`      |
| Bitwise OR                  | or            | or            | \|     | `result = a \| b;`      |
| Bitwise AND                 | b-and         | and           | &      | `result = a & b;`       |
| Bitwise XOR                 | xor           | xor           |xor     | `result = a xor b;`     |
| Bitwise flip                | flip          | not           |flip    | `result = flip(a);`     |
| Maximum                     | max           | max           |max     | `result = max(a, b);`   |
| Minimum                     | min           | min           |min     | `result = min(a, b);`   |
| Angle of vector             | angle         | angle         |angle   | `result = angle(a, b);` |
| Length of vector            | len           | len           |len     | `result = len(a, b);`   |
| 2D simplex noise            | noise         | noise         |noise   | `result = noise(a, b);` |
| Absolute value              | abs           | abs           |abs     | `result = abs(a);`      |
| Natural logarithm           | log           | log           |log     | `result = log(a);`      |
| Base 10 logarithm           | log10         | log10         |log10   | `result = log10(a);`    |
| Floor function              | floor         | floor         |floor   | `result = floor(a);`    |
| Ceiling function            | ceil          | ceil          |ceil    | `result = ceil(a);`     |
| Square root                 | sqrt          | sqrt          |sqrt    | `result = sqrt(a);`     |
| Random decimal \[0, value\] | rand          | rand          |rand    | `result = rand(a);`     |
| Sine                        | sin           | sin           |sin     | `result = sin(a);`      |
| Cosine                      | cos           | cos           |cos     | `result = cos(a);`      |
| Tangent                     | tan           | tan           |tan     | `result = tan(a);`      |
| Arc sine                    | asin          | asin          |asin    | `result = asin(a);`     |
| Arc cosine                  | acos          | acos          |acos    | `result = acos(a);`     |
| Arc tangent                 | atan          | atan          |atan    | `result = atan(a);`     |
