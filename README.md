# Mindustry Logic Compiler
Mindustry Logic Compiler is a compiler for a high-level programming language that compiles to Mindustry Logic.
## Syntax
### for loop
A for loop increments a variable every time it runs and exits once it reaches an exit value. The loop starts at the first number
and exits if the second number is reached after incrementing, meaning the second number is exclusive. If the second number is
less than the first, the loop counts down instead. For example,
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
## Compiler options
