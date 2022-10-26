# Procedural Plant Generation

This project is using a principle called L-Systems which are basically the patterns in which plants split into branches. 

![Alt Text](https://raw.githubusercontent.com/KristinLague/KristinLague.github.io/main/Images/procPlants.gif)

## How does it work?

An L-System always comes with at least two rules one for the character 'X' and one for 'F'. For whatever amount of iterations we want to do we are looping over every character in our string and if we encounter an 'X' or an 'F' then we will replace the character with the rule for said char. This path is then followed applying instructions for each of the characters in the string.

## Example:

**Rules:**

`'X' -> "[F-[X+X]+F[*FX]-X]"`

`'F' -> "FF"`

Every L-System starts with an axion which is basically the starting point of your plant. Usually the axiom is "X". Lets say we are doing only two iterations, that means in the first iteration the axiom looks like this:

`axiom = [F-[X+X]+F[*FX]-X];`

and after the second iteration we are left with this:

`axiom = [FF-[[F-[X+X]+F[*FX]-X]+[F-[X+X]+F[*FX]-X]]+FF[*FF[F-[X+X]+F[*FX]-X]]-[F-[X+X]+F[*FX]-X]];`

This string represents the path that is my plant. Every character in this string will be interpreted into an instruction like this:

- 'F' -> Forward
- '+' -> Rotate Right
- '-' -> Rotate Left
- '*' -> Rotate Forward
- '/' -> Rotate Back
- '[' -> Saves the current position on top of a stack
- ']' -> Returns to last saves position from the stack 

Now this path simple needs to be visualized. 

> This repository contains a way of doing this with lineRenderers which creates two dimensional plants but I've also included a prototype of how something like this could be used to create 3D models. I am basically using a procedurally generated cylinder that I manipulate into the shape of the plant. This isnt perfect by any mean but an interesting prototype for sure.
