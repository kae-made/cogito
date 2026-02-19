# Conceptual Modeling Action Language
## Introduction
### Purpose
The purpose of this manual is to serve as a reference and general user's guide to aid in the correct specification of action semantics for Conceptual Modeling. Although originally designed for models used with the BridgePoint UML Suite, the language described herein can be used to define the action semantics for any UML model in any tool.

### Basic Concepts
### Intended Audience
### Examples

## Language Structure
### Overall Structure
### Comments
### Names and Keywords
### White Space

## Keywords
### Keywords

## Data Items
### Data Items Within an Action
### Modeled Elements
### Implicit Typing
### Local Variables
#### Notes
### Assigning Data Types
### Variable Initialization
#### Examples

### Scoping

### Data Type Strength

## Control Structure

### If Construct
#### Syntax
    // Note that there is no semi-colon following the  
    // "if <boolean expression>"  
     if <boolean expression>
        <statements>
        // Executed if <boolean expression> is TRUE end if;
     if (<boolean expression>)
        <statements>
        // Executed if above boolean expression evaluates to TRUE
    elif (<boolean expression>)
        <statements>
        // Executed if above boolean expression evaluates to TRUE
        // and previous boolean expression is FALSE
    else
        <statements>
        // Executed if both boolean expressions evaluate to FALSE
    end if;
    
    <boolean expression> is an expression evaluating to TRUE or FALSE .
#### Notes

The <i>if</i> construct may contain as many <i>elif</i> clauses as desired.  
Only one <i>else</i> clause may be used, and it must appear at the end of the <i>if</i> construct.

#### Example

### For Each Loop
The for each loop allows for the iteration over a set of instance handles in an instance handle set.

#### Syntax
    for each <instance handle> in <instance handle set> | <ordered instance handle set>
     // Note no semi-colon
        <statements>
    end for;
    
    <instance handle> is a local variable referring to a single instance. <instance handle set> and <ordered instance handle set> is a local variable referring to a set of instance handles.
    an instance in <ordered instance handle set> are retrieved in the order they are arranged.

The statements in the for each construct are executed once against each instance in &lt;instance handle set&gt; .
The order in which the particular instances are processed is undefined. Because the statements in the for each construct can, in principle, be executed in parallel (as when instances are dispersed over multiple processors), the concept of a loop counter is undefined. Consequently, the analyst should not attempt to defeat this restriction.

### Nested Control Logic
#### Examples

## Class Manipulations
### Creating Instances
Creation of an instance of a class is achieved by use of the &lt;create&gt; statement.

#### Syntax
    create class instance <instance handle> of <key letter>;
    create class instance <instance handle>(<property name> : <property value>, ...) of <key letter>;

#### Notes
property should not be reference property
#### Examples

### Selecting Instances
The <i>select</i> statement can be used to assign an instance or set of instances to either an instance handle or a instance handle set respectively. An optional where clause can be used at the end of the <i>select</i> statement to limit the selection. Within the <i>where</i> clause, the selected instance handle refers to each of the instances in the entire set defined by &lt;keyletter&gt; . The instance handle <i>selected</i> is meant to be used as an instance handle in a boolean comparison to form the where expression. The instance or set of instances returned match the criteria of the where expression, and may be empty.

#### Syntax
    select any <instance handle> from instances of <keyletter>;
    select many <instance handle set> from instances of <keyletter>; 
    select any <instance handle> from instances of <keyletter> where <where expression>;
    select many <instance handle set> from instances of <keyletter> where <where expression>;

#### Notes
#### Example

### Writing Attributes

#### Syntax
    [assign] <instance handle>.<attribute> = <expression>;

#### Notes
#### Examples

### Writing Mathematically-Dependent Attributes


### Deleting Instances
    delete object instance <instance handle>;


## Relationships
### Relationship Specifications

#### Syntax
#### Note
#### Examples

### Creating an instance of Relationshp
#### Syntax
    relate <source instance handle> to <destination instance handle> across <relationship specification>;
    relate <source instance handle> to <destination instance handle> across <relationship specification> using <associative instance handle> [[(<property name> : <property value>, ...)] of <key letter>];
#### Note
#### Examples

### Deleting an Instance of a Relationship
#### Syntax
    unrelate <source instance handle> from <destination instance handle> across <relationship specification>;
    unrelate <source instance handle> from <destination instance handle> across <relationship specification> using <associative instance handle>; 
#### Note
#### Examples

### Relationship Navigation
Relationship navigation is the function whereby relationships specified on the Class Diagram are read in order to determine the instance or set of instances that are related to an instance of interest.

#### Syntax
    select one <instance handle> related by <start> -> <relationship link> -> ... <relationship link>;
    select any <instance handle> related by <start> -> <relationship link> -> ... <relationship link>;
    select many <instance handle set> related by <start> -> <relationship link> -> ... <relationship link>;
    select one <instance handle> related by <start> -> <relationship link> -> ... <relationship link> where <where expression>;
    select any <instance handle> related by <start> -> <relationship link> -> ... <relationship link> where <where expression>;
    select many <instance handle set> related by <start> -> <relationship link> -> ... <relationship link> where <where expression>;
    select list <instance handle set> related by <start> -> <relationship link> -> ... <relationship link>; 

## Events
### Receiving Event Data
#### Syntax
#### Note
#### Examples

### Event Generation
#### Syntax
    generate <event label> to <target> [after <timespan>|at <timestamp>];
    generate <event label>:<event meaning> to <target> [after <timespan>|at <timestamp>]; generate <event label> (<event parameters>) to <target> [after <timespan>|at <timestamp>];
    generate <event label>:<event meaning> (<event parameters>) to <target> [after <timespan>|at <timestamp>];
    
    <event label> is <keyletter><event number> .