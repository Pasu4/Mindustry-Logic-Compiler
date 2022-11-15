# Mindustry Logic Compiler
Mindustry Logic Compiler is a compiler for High-Level Mindustry Logic (HLMlog), a high-level programming language that compiles to Mindustry Logic. It tries to make Mindustry Logic easier to write.
## Syntax
### Comments
A comment is opened by ```//``` and closed by a semicolon. Compiler comments are opened with ```///``` and don't show up in the compiled code.
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
The jump keyword jumps to a specified label. A condition can optionally be specified. If a label is at the end of the program, an ```end``` statement is appended.
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
An if statement checks a condition and executes its body if the condition is true. If the scope ends at the end of the program, it appends an ```end``` statement.
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
HLMlog uses slightly different operators from Mlog:
| Mlog (Editor) | Mlog (Export) | HLMlog |
|:-------------:|:-------------:|:------:|
| \+            | add           | \+     |
| \-            | sub           | \-     |
| \*            | mul           | \*     |
| /             | div           | /      |
| //            | idiv          | //     |
| %             | mod           | %      |
| ^             | pow           | ^      |
| ==            | equal         | ==     |
| not           | notEqual      |!=      |
| and           | land          | &&     |
| <             | lessThan      | <      |
| <=            | lessThanEq    | <=     |
| \>            | greaterThan   | \>     |
| \>=           | greaterThanEq | \>=    |
| ===           | strictEqual   | ===    |
| <<            | shl           | <<     |
| \>\>          | shr           | \>\>   |
| or            | or            | \|     |
| b-and         | and           | &      |
| xor           | xor           |xor     |
| flip          | not           |flip    |
| max           | max           |max     |
| min           | min           |min     |
| angle         | angle         |angle   |
| len           | len           |len     |
| noise         | noise         |noise   |
| abs           | abs           |abs     |
| log           | log           |log     |
| log10         | log10         |log10   |
| floor         | floor         |floor   |
| ceil          | ceil          |ceil    |
| sqrt          | sqrt          |sqrt    |
| rand          | rand          |rand    |
| sin           | sin           |sin     |
| cos           | cos           |cos     |
| tan           | tan           |tan     |
| asin          | asin          |asin    |
| acos          | acos          |acos    |
| atan          | atan          |atan    |
