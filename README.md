# Mindustry Logic Compiler
Mindustry Logic Compiler is a compiler for High-Level Mindustry Logic (HLmlog), a high-level programming language that compiles to Mindustry Logic. It aims to make Mindustry Logic easier to write and more human-readable.

## Syntax

### General
In HLmlog as well as in mlog, variables are dynamically typed, meaning the data type or the variable itself do not need to be declared. Every variable's default value is null.

Extra spaces and tabs are ignored. `result = a;` is interpreted the same as <code>result&nbsp;&nbsp;&nbsp;&nbsp; = a;</code>.

Variables should not start with two underscores, since this is the naming convention used by the compiler for flow control.

Every line (excluding compiler options) needs to be terminated by a semicolon (`;`). This includes comments.
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

### jump / label statement
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

#### Example
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

#### Example
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

### sub / return statement
The `sub` statement works like a `jump` statement, but also writes a return point to memory. A `return` statement returns to the last `sub` statement that was called. The `sub` statement thereby acts like a method call. By default, `sub` statements cannot be nested. To change this, use the compiler option `UseStack`.

## Compiler Options
Compiler options tell the compiler how to process your code. To activate a compiler option, add a `#` followed by the name of the compiler option. Lines with compiler options are not with by a semicolon, but with a newline.

### None
This compiler option is implied if no compiler options are specified. It does not change how the compiler runs.
#### Requires
- Nothing
#### Performance
No impact.

### UseStack
Makes use of a data cell to store references to code lines. This is used to nest multiple `sub` statements. Since it uses a data cell, the maximum depth is 64.
#### Requires
- A data cell with the name cell1 linked to the processor
#### Performance
Adds two lines to every `sub` statement and one line to every `return` statement.
#### Example
This code checks if the processor is set up correctly to support the use of nested subs and prints "Setup is working" if the setup is working.
```
#UseStack
sub lbl1;
lbl0 = true;
// Test if setup is correct;
result = lbl0 && lbl1;
result = result && lbl2;
if(result == true)
{
	Print("Setup is working");
	message1.PrintFlush();
}
end;

// Check if depth 1 is reached;
label lbl1;
sub lbl2;
lbl1 = true;
return;

// Check if depth 2 is reached;
label lbl2;
lbl2 = true;
return;
```
In mlog:
```
op add __retAddr @counter 3
write __retAddr cell1 __stack
op add __stack __stack 1
jump 11 always
set lbl0 true
# Test if setup is correct
op land result lbl0 lbl1
op land result result lbl2
jump 10 notEqual result true
print "Setup is working"
printflush message1
end
# Check if depth 1 is reached
op add __retAddr @counter 3
write __retAddr cell1 __stack
op add __stack __stack 1
jump 18 always
set lbl1 true
op sub __stack __stack 1
read @counter cell1 __stack
# Check if depth 2 is reached
set lbl2 true
op sub __stack __stack 1
read @counter cell1 __stack
```

## Coding Examples

### Coordinated fire
This code checks if the player is controlling a linked turret, and if so, makes all turrets mimic the player's actions. The turrets aim at the target of the player and shoot when the player does. When the player leaves the turret, the processor releases control over the turrets. Should be run at least on a medium logic processor if you want to control more than four turrets.
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
### Ore mapper
This script assigns a unit to scan the entire map for resources and map them to a display. It automatically takes screen dimensions, map height and map width into account.
```
// Constants;
scoutUnit = @flare;
stepLength = 10;

// Initialization;
UnitBind(scoutUnit);
x = 0;
y = 0;
Draw(clear, 0, 0, 0);

// Measurements;
screen = Sensor(display1, @type);
screenSize = 80;
if(screen == @large-logic-display) { screenSize = 176; }
scaleX = screenSize / @mapw;
scaleY = screenSize / @maph;
scale = max(scaleX, scaleY);

// Iterate through all positions on the map;
dowhile(true == true)
{
    // Direction to scan the y coordinate;
    up = true;
    dowhile(x < @mapw)
    {
        dowhile(y < @maph)
        {
            sub main;

            y = y + stepLength;
        }
        x = x + stepLength;
        jump stop if x > @mapw;
        // Change direction to avoid extra travel time;
        y = @maph;
        dowhile(y > 0)
        {
            sub main;

            y = y - stepLength;
        }
        x = x + stepLength;
        y = 0;
    }
}

label stop;
stop;

// Bind new unit on death;
label dead;
    UnitBind(scoutUnit);
    jump stop if @unit == null;
jump notDead;

// Main loop;
label main;
    UnitControl(move, x, y);
    arrived = false;
    // Wait until unit has arrived;
    while(arrived != true)
    {
        UnitControl(within, x, y, 2, arrived);
        dead = Sensor(@unit, @dead);
        jump dead if dead == true;
        label notDead;
    }
    // Check for all resources;
    // core, true is a formality for ulocate in mlog;
    UnitLocate(ore, core, true, @copper, outx, outy, found);
    if(found == true)
    { outx = outx * scale; outy = outy * scale; Draw(image, outx, outy, @copper, 8); }
    UnitLocate(ore, core, true, @lead, outx, outy, found);
    if(found == true)
    { outx = outx * scale; outy = outy * scale; Draw(image, outx, outy, @lead, 8); }
    UnitLocate(ore, core, true, @scrap, outx, outy, found);
    if(found == true)
    { outx = outx * scale; outy = outy * scale; Draw(image, outx, outy, @scrap, 8); }
    UnitLocate(ore, core, true, @coal, outx, outy, found);
    if(found == true)
    { outx = outx * scale; outy = outy * scale; Draw(image, outx, outy, @coal, 8); }
    UnitLocate(ore, core, true, @titanium, outx, outy, found);
    if(found == true)
    { outx = outx * scale; outy = outy * scale; Draw(image, outx, outy, @titanium, 8); }
    UnitLocate(ore, core, true, @thorium, outx, outy, found);
    if(found == true)
    { outx = outx * scale; outy = outy * scale; Draw(image, outx, outy, @thorium, 8); }

    display1.DrawFlush();
return;
```
In mlog code:
```
# Constants
set scoutUnit @flare
set stepLength 10
# Initialization
ubind scoutUnit
set x 0
set y 0
draw clear 0 0 0
# Measurements
sensor screen display1 @type
set screenSize 80
jump 10 notEqual screen @large-logic-display
set screenSize 176
op div scaleX screenSize @mapw
op div scaleY screenSize @maph
op max scale scaleX scaleY
# Iterate through all positions on the map
# Direction to scan the y coordinate
set up true
op add __retAddr @counter 1
jump 33
op add y y stepLength
jump 14 lessThan y @maph
op add x x stepLength
jump 29 greaterThan x @mapw
# Change direction to avoid extra travel time
set y @maph
op add __retAddr @counter 1
jump 33
op sub y y stepLength
jump 21 greaterThan y 0
op add x x stepLength
set y 0
jump 14 lessThan x @mapw
jump 13 equal true true
stop
# Bind new unit on death
ubind scoutUnit
jump 29 equal @unit null
jump 39 always
# Main loop
ucontrol move x y
set arrived false
# Wait until unit has arrived
jump 40 equal arrived true
ucontrol within x y 2 arrived
sensor dead @unit @dead
jump 30 equal dead true
jump 35 notEqual arrived true
# Check for all resources
# core, true is a formality for ulocate in mlog
ulocate ore core true @copper outx outy found
jump 45 notEqual found true
op mul outx outx scale
op mul outy outy scale
draw image outx outy @copper 8
ulocate ore core true @lead outx outy found
jump 50 notEqual found true
op mul outx outx scale
op mul outy outy scale
draw image outx outy @lead 8
ulocate ore core true @scrap outx outy found
jump 55 notEqual found true
op mul outx outx scale
op mul outy outy scale
draw image outx outy @scrap 8
ulocate ore core true @coal outx outy found
jump 60 notEqual found true
op mul outx outx scale
op mul outy outy scale
draw image outx outy @coal 8
ulocate ore core true @titanium outx outy found
jump 65 notEqual found true
op mul outx outx scale
op mul outy outy scale
draw image outx outy @titanium 8
ulocate ore core true @thorium outx outy found
jump 70 notEqual found true
op mul outx outx scale
op mul outy outy scale
draw image outx outy @thorium 8
drawflush display1
set @counter __retAddr
```
