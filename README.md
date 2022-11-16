# Mindustry Logic Compiler
Mindustry Logic Compiler is a compiler for High-Level Mindustry Logic (HLmlog), a high-level programming language that compiles to Mindustry Logic. It aims to make Mindustry Logic easier to write and more human-readable.
## Syntax
### General
In HLmlog as well as in mlog, variables are dynamically typed, meaning the data type or the variable itself do not need to be declared. Every variable's default value is null.

Extra spaces and tabs are ignored. `result = a;` is interpreted the same as <code>result&nbsp;&nbsp;&nbsp;&nbsp; = a;</code>.

Every line needs to be terminated by a semicolon (`;`). This includes comments.
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
### while / dowhile loop
A while loop executes its scope until the exit condition is met, after which it exits. It is a pre-test loop. To make it a post-test loop, write `dowhile` instead of `while`.
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
jump 4 lessThanEq i 0
op sub i i 1
jump 1 greaterThan i 0
end
```
With a `dowhile` loop,
```
i = 10;
dowhile(i > 0)
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
### for / dofor loop
A for loop increments a variable every time after it runs its scope and exits once it reaches an exit value. The syntax is `for(<iterator>, <start>, <end>)`. The loop starts at `<start>` and exits if the iterator variable reaches `<end>` after incrementing, which makes it exclusive. It is a pre-test loop. To make it a post-test loop, write `dofor` instead of `for`.
```
j = 0;
for(i, 0, 10)
{
    j = j + i;
}
```
compiles to
```
set j 0
set i 0
jump 6 greaterThanEq i 10
op add j j i
op add i i 1
jump 3 lessThan i 10
end
```
With a `dofor` loop,
```
j = 0;
dofor(i, 0, 10)
{
    j = j + i;
}
```
compiles to
```
set j 0
set i 0
op add j j i
op add i i 1
jump 2 lessThan i 10
```
### Operators
Operators are used to change the value of variables. The syntax varies for each operator, you can find it in the table below. HLmlog also uses slightly different operators from mlog.
| Operator                    | mlog (Editor) | mlog (Export) | HLmlog | Syntax                  |
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

Only one operator can be used per line, writing ´result = a + b + c´ is not possible. Short forms like `result += a;` currently do not work. Operators whithout brackets (eg. \+ or \-) and the equal sign for assignment (`=`) must be separated with spaces from their operand(s), writing `result=10^3` is not currently possible.
## Coding Examples
### Coordinated fire
This code checks if the player is controlling a linked turret, and if so, makes all turres mimic the player's actions. The turrets aim at the target of the player and shoot when the player does. When the player leaves the turret, the processor releases control over the turrets. Should be used at least on a medium logic processor if you want to control more than four turrets.
```
for(i, 0, @links)
{
    // Iterate through every building;
    building = GetLink(i);
    control = Sensor(building, @controlled);

    // Check if a player is controlling the turret;
    if(control == @ctrlPlayer)
    {
        player = building;
        found = 2;
    }

    // Checks if player is still there;
    if(found > 0)
    {
        // Get player action;
        x = Sensor(player, @shootX);
        y = Sensor(player, @shootY);
        shoot = Sensor(player, @shooting);

        //Set turret values;
        building.Control(shoot, x, y, shoot);
    }
}
// Unregister player if not found for two loops;
found = found - 1;
```
The compiled code in mlog looks like this:
```
set i 0
# Iterate through every building
getlink building i
sensor control building @controlled
# Check if a player is controlling the turret
jump 6 notEqual control @ctrlPlayer
set player building
set found 2
# Checks if player is still there
jump 11 lessThanEq found 0
# Get player action
sensor x player @shootX
sensor y player @shootY
sensor shoot player @shooting
# Set turret values
control shoot building x y shoot
op add i i 1
jump 1 lessThan i @links
# Unregister player if not found for two loops
op sub found found 1
```
