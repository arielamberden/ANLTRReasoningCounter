/*
 [The "BSD licence"]
 Copyright (c) 2007-2008 Johannes Luber
 Copyright (c) 2007 Kunle Odutola
 Copyright (c) 2007 Terence Parr
 All rights reserved.

 Redistribution and use in source and binary forms, with or without
 modification, are permitted provided that the following conditions
 are met:
 1. Redistributions of source code must retain the above copyright
    notice, this list of conditions and the following disclaimer.
 2. Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the
    documentation and/or other materials provided with the distribution.
 3. The name of the author may not be used to endorse or promote products
    derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

/** A Java 1.5 grammar for ANTLR v3 derived from the spec
 *
 *  This is a very close representation of the spec; the changes
 *  are comestic (remove left recursion) and also fixes (the spec
 *  isn't exactly perfect).  I have run this on the 1.4.2 source
 *  and some nasty looking enums from 1.5, but have not really
 *  tested for 1.5 compatibility.
 *
 *  I built this with: java -Xmx100M org.antlr.Tool java.g
 *  and got two errors that are ok (for now):
 *  java.g:691:9: Decision can match input such as
 *    "'0'..'9'{'E', 'e'}{'+', '-'}'0'..'9'{'D', 'F', 'd', 'f'}"
 *    using multiple alternatives: 3, 4
 *  As a result, alternative(s) 4 were disabled for that input
 *  java.g:734:35: Decision can match input such as "{'$', 'A'..'Z',
 *    '_', 'a'..'z', '\u00C0'..'\u00D6', '\u00D8'..'\u00F6',
 *    '\u00F8'..'\u1FFF', '\u3040'..'\u318F', '\u3300'..'\u337F',
 *    '\u3400'..'\u3D2D', '\u4E00'..'\u9FFF', '\uF900'..'\uFAFF'}"
 *    using multiple alternatives: 1, 2
 *  As a result, alternative(s) 2 were disabled for that input
 *
 *  [ 
 *    Kunle: Above didn't work for me, this did:
 *             java -Xmx200M org.antlr.Tool java.g 
             OR
 *             java -Xms200M -Xmx200M org.antlr.Tool java.g 
 *  ]

 *  You can turn enum on/off as a keyword :)
 *
 *  Version 1.0 -- initial release July 5, 2006 (requires 3.0b2 or higher)
 *
 *  Primary author: Terence Parr, July 2006
 *
 *  C# version: Kunle Odutola, July 2006
 *
 *  Version 1.0.1 -- corrections by Koen Vanderkimpen & Marko van Dooren,
 *      October 25, 2006;
 *      fixed normalInterfaceDeclaration: now uses typeParameters instead
 *          of typeParameter (according to JLS, 3rd edition)
 *      fixed castExpression: no longer allows expression next to type
 *          (according to semantics in JLS, in contrast with syntax in JLS)
 *
 *  Version 1.0.2 -- Terence Parr, Nov 27, 2006
 *      java spec I built this from had some bizarre for-loop control.
 *          Looked weird and so I looked elsewhere...Yep, it's messed up.
 *          simplified.
 *
 *  Version 1.0.3 -- Chris Hogue, Feb 26, 2007
 *      Factored out an annotationName rule and used it in the annotation rule.
 *          Not sure why, but typeName wasn't recognizing references to inner
 *          annotations (e.g. @InterfaceName.InnerAnnotation())
 *      Factored out the elementValue section of an annotation reference.  Created 
 *          elementValuePair and elementValuePairs rules, then used them in the 
 *          annotation rule.  Allows it to recognize annotation references with 
 *          multiple, comma separated attributes.
 *      Updated elementValueArrayInitializer so that it allows multiple elements.
 *          (It was only allowing 0 or 1 element).
 *      Updated localVariableDeclaration to allow annotations.  Interestingly the JLS
 *          doesn't appear to indicate this is legal, but it does work as of at least
 *          JDK 1.5.0_06.
 *      Moved the Identifier portion of annotationTypeElementRest to annotationMethodRest.
 *          Because annotationConstantRest already references variableDeclarator which 
 *          has the Identifier portion in it, the parser would fail on constants in 
 *          annotation definitions because it expected two identifiers.  
 *      Added optional trailing ';' to the alternatives in annotationTypeElementRest.
 *          Wouldn't handle an inner interface that has a trailing ';'.
 *      Swapped the expression and type rule reference order in castExpression to 
 *          make it check for genericized casts first.  It was failing to recognize a
 *          statement like  "Class<Byte> TYPE = (Class<Byte>)...;" because it was seeing
 *          'Class<Byte' in the cast expression as a less than expression, then failing 
 *          on the '>'.
 *      Changed createdName to use typeArguments instead of nonWildcardTypeArguments.
 *      Changed the 'this' alternative in primary to allow 'identifierSuffix' rather than
 *          just 'arguments'.  The case it couldn't handle was a call to an explicit
 *          generic method invocation (e.g. this.<E>doSomething()).  Using identifierSuffix
 *          may be overly aggressive--perhaps should create a more constrained thisSuffix rule?
 * 
 *  Version 1.0.4 -- Hiroaki Nakamura, May 3, 2007
 *
 *  Fixed formalParameterDecls, localVariableDeclaration, forInit,
 *  and forVarControl to use variableModifier* not 'final'? (annotation)?
 *
 *  Version 1.0.5 -- Terence, June 21, 2007
 *  --a[i].foo didn't work. Fixed unaryExpression
 *
 *  Version 1.0.6 -- John Ridgway, March 17, 2008
 *      Made "assert" a switchable keyword like "enum".
 *      Fixed compilationUnit to disallow "annotation importDeclaration ...".
 *      Changed "Identifier ('.' Identifier)*" to "qualifiedName" in more 
 *          places.
 *      Changed modifier* and/or variableModifier* to classOrInterfaceModifiers,
 *          modifiers or variableModifiers, as appropriate.
 *      Renamed "bound" to "typeBound" to better match language in the JLS.
 *      Added "memberDeclaration" which rewrites to methodDeclaration or 
 *      fieldDeclaration and pulled type into memberDeclaration.  So we parse 
 *          type and then move on to decide whether we're dealing with a field
 *          or a method.
 *      Modified "constructorDeclaration" to use "constructorBody" instead of
 *          "methodBody".  constructorBody starts with explicitConstructorInvocation,
 *          then goes on to blockStatement*.  Pulling explicitConstructorInvocation
 *          out of expressions allowed me to simplify "primary".
 *      Changed variableDeclarator to simplify it.
 *      Changed type to use classOrInterfaceType, thus simplifying it; of course
 *          I then had to add classOrInterfaceType, but it is used in several 
 *          places.
 *      Fixed annotations, old version allowed "@X(y,z)", which is illegal.
 *      Added optional comma to end of "elementValueArrayInitializer"; as per JLS.
 *      Changed annotationTypeElementRest to use normalClassDeclaration and 
 *          normalInterfaceDeclaration rather than classDeclaration and 
 *          interfaceDeclaration, thus getting rid of a couple of grammar ambiguities.
 *      Split localVariableDeclaration into localVariableDeclarationStatement
 *          (includes the terminating semi-colon) and localVariableDeclaration.  
 *          This allowed me to use localVariableDeclaration in "forInit" clauses,
 *           simplifying them.
 *      Changed switchBlockStatementGroup to use multiple labels.  This adds an
 *          ambiguity, but if one uses appropriately greedy parsing it yields the
 *           parse that is closest to the meaning of the switch statement.
 *      Renamed "forVarControl" to "enhancedForControl" -- JLS language.
 *      Added semantic predicates to test for shift operations rather than other
 *          things.  Thus, for instance, the string "< <" will never be treated
 *          as a left-shift operator.
 *      In "creator" we rule out "nonWildcardTypeArguments" on arrayCreation, 
 *          which are illegal.
 *      Moved "nonWildcardTypeArguments into innerCreator.
 *      Removed 'super' superSuffix from explicitGenericInvocation, since that
 *          is only used in explicitConstructorInvocation at the beginning of a
 *           constructorBody.  (This is part of the simplification of expressions
 *           mentioned earlier.)
 *      Simplified primary (got rid of those things that are only used in
 *          explicitConstructorInvocation).
 *      Lexer -- removed "Exponent?" from FloatingPointLiteral choice 4, since it
 *          led to an ambiguity.
 *
 *      This grammar successfully parses every .java file in the JDK 1.5 source 
 *          tree (excluding those whose file names include '-', which are not
 *          valid Java compilation units).
 *
 *  June 26, 2008
 *
 *	conditionalExpression had wrong precedence x?y:z.
 *
 *  Known remaining problems:
 *      "Letter" and "JavaIDDigit" are wrong.  The actual specification of
 *      "Letter" should be "a character for which the method
 *      Character.isJavaIdentifierStart(int) returns true."  A "Java 
 *      letter-or-digit is a character for which the method 
 *      Character.isJavaIdentifierPart(int) returns true."
 *
 */		
grammar Java_MIT;

options {
	language=CSharp2;
	backtrack=true;
	memoize=true;
}

@lexer::members {
	protected bool enumIsKeyword = true;
	protected bool assertIsKeyword = true;
	
	// Data fields
	public int charCount = 0;
	public int whiteSpaceCount = 0;
	public int commentCharCount = 0;
	
	// Data accessors
	public int getCharCount(){ return charCount; }
	public int getWhiteSpaceCount(){ return whiteSpaceCount; }
	public int getCommentCharCount(){ return commentCharCount; }
}

@members {	
	// Scope variables
	public string currentPackage = "OutsideOfPackage";
	public string currentClass = "OutsideOfClass";
	public string currentMethod = "OutsideOfMethod";
	
	// Data fields
	public Dictionary<string,Package> packages = new Dictionary<string,Package>(){{"OutsideOfPackage", new Package("OutsideOfPackage")}};
	public Dictionary<string,Class> classes = new Dictionary<string,Class>(){{"OutsideOfClass",new Class("OutsideOfClass","OutsideOfPackage")}};
	public Dictionary<string,Method> methods = new Dictionary<string,Method>(){{"OutsideOfMethod",new Method("OutsideOfMethod","OutsideOfPackage","OutsideOfClass")}};

	// Data accessors
	public Dictionary<string,Package> getPackages(){ return packages; }
	public Dictionary<string,Class> getClasses(){ return classes; }
	public Dictionary<string,Method> getMethods(){ return methods; }
	
	// Add packages, classes, methods
	public void AddPackage(string name)
	{
		packages.Add(name,new Package(name));
		currentPackage = name;
	}
	public void AddClass(string name)
	{
		classes.Add(name,new Class(name,currentPackage));
		currentClass = name;
	}
	public void AddMethod(string name)
	{
		methods.Add(name,new Method(name,currentPackage,currentClass));
		currentMethod = name;
	}
	
	// Record a keyword, UDI, constant, or special character in package, class, and method
	public void RecordKeyword(string k)
	{
		packages[currentPackage].addKeyword(k);
		classes[currentClass].addKeyword(k);
		methods[currentMethod].addKeyword(k);
	}
	public void RecordUserDefinedIdentifier(string k)
	{
		packages[currentPackage].addUserDefinedIdentifier(k);
		classes[currentClass].addUserDefinedIdentifier(k);
		methods[currentMethod].addUserDefinedIdentifier(k);
	}
	public void RecordConstant(string k)
	{
		packages[currentPackage].addConstant(k);
		classes[currentClass].addConstant(k);
		methods[currentMethod].addConstant(k);
	}
	public void RecordSpecialCharacter(char k)
	{
		packages[currentPackage].addSpecialCharacter(k);
		classes[currentClass].addSpecialCharacter(k);
		methods[currentMethod].addSpecialCharacter(k);
	}	
}

@namespace { ANTLRReasoningCounter }

// starting point for parsing a java file
/*keywordRecorder 
	:	'abstract' 	{ RecordKeyword("abstract"); } 
	|  	'assert'	{ RecordKeyword("assert"); }
	|	'boolean'	{ RecordKeyword("boolean"); }
    	| 	'break'		{ RecordKeyword("break"); }
	|	'byte'		{ RecordKeyword("byte"); }
	|	'case'		{ RecordKeyword("case"); }
	|	'catch'		{ RecordKeyword("catch"); }
	|	'char'		{ RecordKeyword("char"); }
	|	'class'		{ RecordKeyword("class"); }
	|	'const'		{ RecordKeyword("const"); }
	|	'continue'	{ RecordKeyword("continue"); }
	|	'default'	{ RecordKeyword("default"); }
	|	'do'		{ RecordKeyword("do"); }
	|	'double'	{ RecordKeyword("double"); }
	|	'else'		{ RecordKeyword("else"); }
	|	'enum'		{ RecordKeyword("enum"); }
	|	'extends'	{ RecordKeyword("extends"); }
	|	'final'		{ RecordKeyword("final"); }
	|	'finally'	{ RecordKeyword("finally"); }
	|	'float'		{ RecordKeyword("float"); }
	|	'for'		{ RecordKeyword("for"); }
	|	'goto'		{ RecordKeyword("goto"); }
	|	'if'		{ RecordKeyword("if"); }
	|	'implements'	{ RecordKeyword("implements"); }
	|	'import'	{ RecordKeyword("import"); }
	|	'instanceof'	{ RecordKeyword("instanceof"); }
	|	'int'		{ RecordKeyword("int"); }
	|	'interface'	{ RecordKeyword("interfacetract"); }
	|	'long'		{ RecordKeyword("long"); }
	|	'native'	{ RecordKeyword("native"); }
	|	'new'		{ RecordKeyword("new"); }
	|	'package'	{ RecordKeyword("package"); }
	|	'private'	{ RecordKeyword("private"); }
	|	'protected'	{ RecordKeyword("protec"); }
	|	'public'	{ RecordKeyword("public"); }
	|	'return'	{ RecordKeyword("return"); }
	|	'short'		{ RecordKeyword("short"); }
	|	'static'	{ RecordKeyword("static"); }
	|	'strictfp'	{ RecordKeyword("strictfp"); }
	|	'super'		{ RecordKeyword("super"); }
	|	'switch'	{ RecordKeyword("switch"); }
	|	'synchronized'	{ RecordKeyword("synchronized"); }
	|	'this'		{ RecordKeyword("this"); }
	|	'throw'		{ RecordKeyword("throw"); }
	|	'throws'	{ RecordKeyword("throws"); }
	|	'transient'	{ RecordKeyword("transient"); }
	|	'try'		{ RecordKeyword("try"); }
	|	'void'		{ RecordKeyword("void"); }
	|	'volatile'	{ RecordKeyword("volatile"); }
	|	'while'		{ RecordKeyword("while"); }
	|	'true'		{ RecordKeyword("true"); }
	|	'false'		{ RecordKeyword("false"); }
	|	'null'  	{ RecordKeyword("null"); }
	; 
*/

/* The annotations are separated out to make parsing faster, but must be associated with
   a packageDeclaration or a typeDeclaration (and not an empty one). */
compilationUnit
    :   
    	(annotations
        (   packageDeclaration importDeclaration* typeDeclaration*
        |   classOrInterfaceDeclaration typeDeclaration*
        )
    	|   packageDeclaration? importDeclaration* typeDeclaration*
    	)
    ;

packageDeclaration
    :   'package' x=qualifiedName ';' { RecordKeyword("package"); RecordSpecialCharacter(';'); AddPackage($x.value); }
    ;
    
importDeclaration
    :   'import' 'static'? qualifiedName ('.' '*')? ';' {RecordKeyword("import"); RecordSpecialCharacter(';'); }
    ;
    
typeDeclaration
    :   classOrInterfaceDeclaration
    |   ';'
    ;
    
classOrInterfaceDeclaration
    :   classOrInterfaceModifiers (classDeclaration | interfaceDeclaration)
    ;
    
classOrInterfaceModifiers
    :   classOrInterfaceModifier*
    ;

classOrInterfaceModifier
    :   annotation   // class or interface
    |   'public'     {RecordKeyword("public");}// class or interface
    |   'protected'  {RecordKeyword("protected");}// class or interface
    |   'private'    {RecordKeyword("private");}// class or interface
    |   'abstract'   {RecordKeyword("abstract");}// class or interface
    |   'static'     {RecordKeyword("static");}// class or interface
    |   'final'      {RecordKeyword("final");}// class only -- does not apply to interfaces
    |   'strictfp'   {RecordKeyword("strictfp");}// class or interface
    ;

modifiers
    :   modifier*
    ;

classDeclaration
    :   normalClassDeclaration
    |   enumDeclaration
    ;
    
normalClassDeclaration
    :   modifiers 'class' Identifier { RecordKeyword("class"); AddClass($Identifier.text);  RecordSpecialCharacter('{'); RecordSpecialCharacter('}');}
    	typeParameters?
        ('extends' type)? 
        ('implements' typeList)?
        classBody
    ;
    
typeParameters
    :   '<' typeParameter (',' typeParameter { RecordSpecialCharacter(','); })* '>' 
    { RecordSpecialCharacter('<'); RecordSpecialCharacter('>');}
    ;

typeParameter
    :   Identifier ('extends' typeBound)? { RecordKeyword("extends"); }
    ;
        
typeBound
    :   type ('&' type { RecordSpecialCharacter('&'); })*
    ;

enumDeclaration
    :   ENUM Identifier ('implements' typeList)? enumBody { RecordUserDefinedIdentifier($Identifier.text); RecordKeyword("implements"); }
    ;

enumBody
    :   '{' enumConstants? ','? enumBodyDeclarations? '}' { RecordSpecialCharacter('{');RecordSpecialCharacter(',');RecordSpecialCharacter('}'); }
    ;

enumConstants
    :   enumConstant (',' enumConstant { RecordSpecialCharacter(','); })*
    ;
    
enumConstant
    :   annotations? Identifier arguments? classBody? { RecordUserDefinedIdentifier($Identifier.text); }
    ;
    
enumBodyDeclarations
    :   ';' (classBodyDeclaration)* { RecordSpecialCharacter(';'); }
    ;
    
interfaceDeclaration
    :   normalInterfaceDeclaration
    |   annotationTypeDeclaration
    ;
    
normalInterfaceDeclaration
    :   'interface' Identifier typeParameters? { RecordKeyword("interface");RecordUserDefinedIdentifier($Identifier.text); }
    ('extends' typeList)? 
    { RecordKeyword("extends"); }
    interfaceBody 
    ;
    
typeList
    :   type (',' type { RecordSpecialCharacter(','); })*
    ;
    
classBody
    :   '{' classBodyDeclaration* '}' //{ RecordSpecialCharacter('{'); RecordSpecialCharacter('}'); }
    ;
    
interfaceBody
    :   '{' interfaceBodyDeclaration* '}' //{ RecordSpecialCharacter('{'); RecordSpecialCharacter('}'); }
    ;

classBodyDeclaration
    :   ';' {RecordSpecialCharacter(';');}
    |   'static'? block {RecordKeyword("static");}
    |   modifiers memberDecl
    ;
    
memberDecl
    :   genericMethodOrConstructorDecl
    |   memberDeclaration
    |   'void' Identifier { RecordKeyword("void");AddMethod($Identifier.text); RecordSpecialCharacter('{'); RecordSpecialCharacter('}');} voidMethodDeclaratorRest
    |   Identifier { AddMethod($Identifier.text); RecordSpecialCharacter('{'); RecordSpecialCharacter('}');} constructorDeclaratorRest
    |   interfaceDeclaration
    |   classDeclaration
    ;
    
memberDeclaration
    :   type (methodDeclaration | fieldDeclaration)
    ;

genericMethodOrConstructorDecl
    :   typeParameters genericMethodOrConstructorRest
    ;
    
genericMethodOrConstructorRest
    :   (type | 'void') Identifier { AddMethod($Identifier.text); } methodDeclaratorRest
    |   Identifier { AddMethod($Identifier.text); } constructorDeclaratorRest
    ;

methodDeclaration
    :   Identifier { AddMethod($Identifier.text); } methodDeclaratorRest 
        ;

fieldDeclaration
    :   variableDeclarators ';' {RecordSpecialCharacter(';');}
    ;
        
interfaceBodyDeclaration
    :   modifiers interfaceMemberDecl
    |   ';' {RecordSpecialCharacter(';');}
    ;

interfaceMemberDecl
    :   interfaceMethodOrFieldDecl
    |   interfaceGenericMethodDecl
    |   'void' Identifier { RecordKeyword("void"); RecordUserDefinedIdentifier($Identifier.text); } voidInterfaceMethodDeclaratorRest
    |   interfaceDeclaration
    |   classDeclaration
    ;
    
interfaceMethodOrFieldDecl
    :   type Identifier {RecordUserDefinedIdentifier($Identifier.text);} interfaceMethodOrFieldRest
    ;
    
interfaceMethodOrFieldRest
    :   constantDeclaratorsRest ';' {RecordSpecialCharacter(';');}
    |   interfaceMethodDeclaratorRest
    ;
    
methodDeclaratorRest
    :   formalParameters ('[' ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');})*
        ('throws' qualifiedNameList {RecordKeyword("throws");})?
        (   methodBody
        |   ';' {RecordSpecialCharacter(';');}
        )
    ;
    
voidMethodDeclaratorRest
    :   formalParameters ('throws' qualifiedNameList {RecordKeyword("throws");} )?
        (   methodBody
        |   ';' {RecordSpecialCharacter(';');}
        )
    ;
    
interfaceMethodDeclaratorRest
    :   formalParameters ('[' ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');})* ('throws' qualifiedNameList {RecordKeyword("throws");})? ';' { RecordSpecialCharacter(';');}
    ;
    
interfaceGenericMethodDecl
    :   typeParameters (type | 'void' {RecordKeyword("void");}) Identifier {RecordUserDefinedIdentifier($Identifier.text);}
        interfaceMethodDeclaratorRest
    ;
    
voidInterfaceMethodDeclaratorRest
    :   formalParameters ('throws' qualifiedNameList {RecordKeyword("throws");})? ';' {RecordSpecialCharacter(';');}
    ;
    
constructorDeclaratorRest
    :   formalParameters ('throws' qualifiedNameList {RecordKeyword("throws");})? constructorBody
    ;

constantDeclarator
    :   Identifier {RecordUserDefinedIdentifier($Identifier.text);} constantDeclaratorRest
    ;
    
variableDeclarators
    :   variableDeclarator (',' variableDeclarator {RecordSpecialCharacter(',');})*
    ;

variableDeclarator
    :   variableDeclaratorId ('=' {RecordSpecialCharacter('=');} variableInitializer)? //{RecordSpecialCharacter('=');}
    ;
    
constantDeclaratorsRest
    :   constantDeclaratorRest (',' constantDeclarator {RecordSpecialCharacter(',');})*
    ;

constantDeclaratorRest
    :   ('[' ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');})* '=' variableInitializer {RecordSpecialCharacter('=');}
    ;
    
variableDeclaratorId
    :   Identifier {RecordUserDefinedIdentifier($Identifier.text);} ('[' ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');})*
    ;

variableInitializer
    :   arrayInitializer
    |   expression
    ;
        
arrayInitializer
    :   '{' (variableInitializer (',' variableInitializer {RecordSpecialCharacter(',');})* (',')? )? '}' { RecordSpecialCharacter('{'); RecordSpecialCharacter('}'); }
    ;

modifier
    :   annotation
    |   'public' { RecordKeyword("public"); }
    |   'protected' { RecordKeyword("protected"); }
    |   'private' { RecordKeyword("private"); }
    |   'static' { RecordKeyword("static"); }
    |   'abstract' { RecordKeyword("abstract"); }
    |   'final' { RecordKeyword("final"); }
    |   'native' { RecordKeyword("native"); }
    |   'synchronized' { RecordKeyword("synchronized"); }
    |   'transient' { RecordKeyword("transient"); }
    |   'volatile' { RecordKeyword("volatile"); }
    |   'strictfp' { RecordKeyword("strictfp"); }
    ;

packageOrTypeName
    :   qualifiedName
    ;

enumConstantName
    :   Identifier {RecordUserDefinedIdentifier($Identifier.text);}
    ;

typeName
    :   qualifiedName
    ;

type
	:	classOrInterfaceType ('[' ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');})*
	|	primitiveType ('[' ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');})*
	;

classOrInterfaceType
	:	Identifier typeArguments? ('.' Identifier typeArguments? )*
	;

primitiveType
    :   'boolean' { RecordKeyword("boolean"); }
    |   'char' { RecordKeyword("char"); }
    |   'byte' { RecordKeyword("byte"); }
    |   'short' { RecordKeyword("short"); }
    |   'int' { RecordKeyword("int"); }
    |   'long' { RecordKeyword("long"); }
    |   'float' { RecordKeyword("float"); }
    |   'double' { RecordKeyword("double"); }
    ;

variableModifier
    :   'final' { RecordKeyword("final"); }
    |   annotation
    ;

typeArguments
    :   '<' typeArgument (',' typeArgument {RecordSpecialCharacter(',');})* '>' { RecordSpecialCharacter('<'); RecordSpecialCharacter('>');}
    ;
    
typeArgument
    :   type
    |   '?' {RecordSpecialCharacter('?');} (('extends' { RecordKeyword("extends"); } | 'super' { RecordKeyword("super"); }) type)?
    ;
    
qualifiedNameList
    :   qualifiedName (',' qualifiedName {RecordSpecialCharacter(',');})*
    ;

formalParameters
    :   '(' formalParameterDecls? ')' { RecordSpecialCharacter('('); RecordSpecialCharacter(')'); }
    ;
    
formalParameterDecls
    :   variableModifiers type formalParameterDeclsRest
    ;
    
formalParameterDeclsRest
    :   variableDeclaratorId (',' formalParameterDecls {RecordSpecialCharacter(',');})?
    |   '...' variableDeclaratorId {RecordSpecialCharacter('.');RecordSpecialCharacter('.');RecordSpecialCharacter('.');}
    ;
    
methodBody
    :   block //{ RecordSpecialCharacter('{'); RecordSpecialCharacter('}'); }
    ;

constructorBody
    :   '{' explicitConstructorInvocation? blockStatement* '}'
    ;

explicitConstructorInvocation
    :   nonWildcardTypeArguments? ('this' { RecordKeyword("this"); } | 'super' { RecordKeyword("super"); }) arguments ';'
    |   primary '.' {RecordSpecialCharacter('.');} nonWildcardTypeArguments? 'super' { RecordKeyword("super"); } arguments ';' {RecordSpecialCharacter(';');}
    ;


qualifiedName returns [string value]
    :   id=Identifier { $value = $id.text; }
        ('.' id=Identifier { $value += "." + $id.text; { RecordSpecialCharacter('.'); }}
        )*
    ;
    
literal 
    :   integerLiteral
    |   fpl=FloatingPointLiteral {RecordConstant($fpl.text);}
    |   cl=CharacterLiteral {RecordConstant($cl.text);}
    |   sl=StringLiteral {RecordConstant($sl.text);}
    |   booleanLiteral
    |   'null' { RecordKeyword("null"); }
    ;

integerLiteral
    :   hl=HexLiteral {RecordConstant($hl.text);}
    |   ol=OctalLiteral {RecordConstant($ol.text);}
    |   dl=DecimalLiteral {RecordConstant($dl.text);}
    ;

booleanLiteral
    :   'true' { RecordKeyword("true"); }
    |   'false' { RecordKeyword("false"); }
    ; 

// ANNOTATIONS

annotations
    :   annotation+
    ;

annotation
    :   '@' {RecordSpecialCharacter('@');} annotationName ( '(' ( elementValuePairs | elementValue )? ')' { RecordSpecialCharacter('('); RecordSpecialCharacter(')'); } )?
    ;
    
annotationName
    : Identifier ('.' Identifier)*
    ;

elementValuePairs
    :   elementValuePair (',' elementValuePair {RecordSpecialCharacter(',');})*
    ;

elementValuePair
    :   Identifier '=' elementValue {RecordUserDefinedIdentifier($Identifier.text);RecordSpecialCharacter('=');}
    ;
    
elementValue
    :   conditionalExpression
    |   annotation
    |   elementValueArrayInitializer
    ;
    
elementValueArrayInitializer
    :   '{' (elementValue (',' elementValue {RecordSpecialCharacter(',');})*)? (',' {RecordSpecialCharacter(',');})? '}' { RecordSpecialCharacter('{'); RecordSpecialCharacter('}'); }
    ;
    
annotationTypeDeclaration
    :   '@' 'interface' Identifier annotationTypeBody {RecordSpecialCharacter('@'); RecordKeyword("interface");}
    ;
    
annotationTypeBody
    :   '{' (annotationTypeElementDeclaration)* '}' { RecordSpecialCharacter('{'); RecordSpecialCharacter('}'); }
    ;
    
annotationTypeElementDeclaration
    :   modifiers annotationTypeElementRest
    ;
    
annotationTypeElementRest
    :   type annotationMethodOrConstantRest ';' {RecordSpecialCharacter(';');}
    |   {RecordSpecialCharacter(';');} normalClassDeclaration ';' ?
    |   {RecordSpecialCharacter(';');} normalInterfaceDeclaration ';' ?
    |   {RecordSpecialCharacter(';');} enumDeclaration ';' ?
    |   {RecordSpecialCharacter(';');} annotationTypeDeclaration ';'?
    ;
    
annotationMethodOrConstantRest
    :   annotationMethodRest
    |   annotationConstantRest
    ;
    
annotationMethodRest
    :   Identifier '(' ')' defaultValue? { RecordSpecialCharacter('('); RecordSpecialCharacter(')'); }
    ;
    
annotationConstantRest
    :   variableDeclarators
    ;
    
defaultValue
    :   'default' elementValue {RecordKeyword("default");}
    ;

// STATEMENTS / BLOCKS

block
    :   '{' blockStatement* '}' //{ RecordSpecialCharacter('{'); RecordSpecialCharacter('}'); }
    ;
    
blockStatement
    :   localVariableDeclarationStatement
    |   classOrInterfaceDeclaration
    |   statement
    ;
    
localVariableDeclarationStatement
    :    localVariableDeclaration ';' {RecordSpecialCharacter(';');}
    ;

localVariableDeclaration
    :   variableModifiers type variableDeclarators
    ;
    
variableModifiers
    :   variableModifier*
    ;

statement
    : block
    |   ASSERT expression (':' expression {RecordSpecialCharacter(':');})? ';' {RecordSpecialCharacter(';');}
    |   'if' parExpression statement (options {k=1;}:'else' statement)?
    |   'for' '(' forControl ')' statement { RecordSpecialCharacter('('); RecordSpecialCharacter(')'); }
    |   'while' parExpression statement
    |   'do' statement 'while' parExpression ';' {RecordSpecialCharacter(';');}
    |   'try' block {RecordKeyword("try");}
        ( catches 'finally' block {RecordKeyword("finally");}
        | catches
        |   'finally' block {RecordKeyword("finally");}
        )
    |   'switch' parExpression '{' switchBlockStatementGroups '}' {RecordKeyword("switch"); RecordSpecialCharacter('{'); RecordSpecialCharacter('}'); }
    |   'synchronized' parExpression block {RecordKeyword("synchronized");}
    |   'return' expression? ';' {RecordKeyword("return");RecordSpecialCharacter(';');}
    |   'throw' expression ';' {RecordSpecialCharacter(';'); RecordKeyword("throw");}
    |   'break' Identifier? ';' {RecordKeyword("break");RecordUserDefinedIdentifier($Identifier.text);RecordSpecialCharacter(';');}
    |   'continue' Identifier? ';' {RecordKeyword("continue");RecordUserDefinedIdentifier($Identifier.text);RecordSpecialCharacter(';');}
    |   ';' {RecordSpecialCharacter(';');}
    |   statementExpression ';' {RecordSpecialCharacter(';');}
    |   Identifier ':' statement {RecordUserDefinedIdentifier($Identifier.text);RecordSpecialCharacter(':');}
    ;
    
catches
    :   catchClause (catchClause)*
    ;
    
catchClause
    :   'catch' '(' formalParameter ')' block {RecordKeyword("catch"); RecordSpecialCharacter('('); RecordSpecialCharacter(')'); }
    ;

formalParameter
    :   variableModifiers type variableDeclaratorId
    ;
        
switchBlockStatementGroups
    :   (switchBlockStatementGroup)*
    ;
    
/* The change here (switchLabel -> switchLabel+) technically makes this grammar
   ambiguous; but with appropriately greedy parsing it yields the most
   appropriate AST, one in which each group, except possibly the last one, has
   labels and statements. */
switchBlockStatementGroup
    :   switchLabel+ blockStatement*
    ;
    
switchLabel
    :   'case' constantExpression ':' {RecordSpecialCharacter(':'); RecordKeyword("case");}
    |   'case' enumConstantName ':' {RecordSpecialCharacter(':'); RecordKeyword("case");}
    |   'default' ':' {RecordSpecialCharacter(':'); RecordKeyword("default");}
    ;
    
forControl
options {k=3;} // be efficient for common case: for (ID ID : ID) ...
    :   enhancedForControl
    |   forInit? ';' expression? ';' forUpdate? {RecordSpecialCharacter(';');RecordSpecialCharacter(';');}
    ; 

forInit
    :   localVariableDeclaration
    |   expressionList
    ;
    
enhancedForControl
    :   variableModifiers type Identifier ':' expression {RecordUserDefinedIdentifier($Identifier.text);RecordSpecialCharacter(':');}
    ;

forUpdate
    :   expressionList
    ;

// EXPRESSIONS

parExpression
    :   '(' expression ')' { RecordSpecialCharacter('('); RecordSpecialCharacter(')'); }
    ;
    
expressionList
    :   expression (',' expression {RecordSpecialCharacter(',');})*
    ;

statementExpression
    :   expression
    ;
    
constantExpression
    :   expression
    ;
    
expression
    :   conditionalExpression (assignmentOperator expression)?
    ;
    
assignmentOperator
    :   '=' {RecordSpecialCharacter('=');}
    |   '+=' {RecordSpecialCharacter('+');RecordSpecialCharacter('=');}
    |   '-=' {RecordSpecialCharacter('-');RecordSpecialCharacter('=');}
    |   '*=' {RecordSpecialCharacter('*');RecordSpecialCharacter('=');}
    |   '/=' {RecordSpecialCharacter('/');RecordSpecialCharacter('=');}
    |   '&=' {RecordSpecialCharacter('&');RecordSpecialCharacter('=');}
    |   '|=' {RecordSpecialCharacter('|');RecordSpecialCharacter('=');}
    |   '^=' {RecordSpecialCharacter('^');RecordSpecialCharacter('=');}
    |   '%=' {RecordSpecialCharacter('\%');RecordSpecialCharacter('=');}
    |   ('<' '<' '=')=> t1='<' t2='<' t3='=' 
        { $t1.Line == $t2.Line &&
          $t1.CharPositionInLine + 1 == $t2.CharPositionInLine && 
          $t2.Line == $t3.Line && 
          $t2.CharPositionInLine + 1 == $t3.CharPositionInLine }?
    |   ('>' '>' '>' '=')=> t1='>' t2='>' t3='>' t4='='
        { $t1.Line == $t2.Line && 
          $t1.CharPositionInLine + 1 == $t2.CharPositionInLine &&
          $t2.Line == $t3.Line && 
          $t2.CharPositionInLine + 1 == $t3.CharPositionInLine &&
          $t3.Line == $t4.Line && 
          $t3.CharPositionInLine + 1 == $t4.CharPositionInLine }?
    |   ('>' '>' '=')=> t1='>' t2='>' t3='='
        { $t1.Line == $t2.Line && 
          $t1.CharPositionInLine + 1 == $t2.CharPositionInLine && 
          $t2.Line == $t3.Line && 
          $t2.CharPositionInLine + 1 == $t3.CharPositionInLine }?
    ;

conditionalExpression
    :   conditionalOrExpression ( '?' conditionalExpression ':' conditionalExpression {RecordSpecialCharacter('?');RecordSpecialCharacter(':');})?
    ;

conditionalOrExpression
    :   conditionalAndExpression ( '||' conditionalAndExpression {RecordSpecialCharacter('|');RecordSpecialCharacter('|');})*
    ;

conditionalAndExpression
    :   inclusiveOrExpression ( '&&' inclusiveOrExpression {RecordSpecialCharacter('&');RecordSpecialCharacter('&');})*
    ;

inclusiveOrExpression
    :   exclusiveOrExpression ( '|' exclusiveOrExpression {RecordSpecialCharacter('|');})*
    ;

exclusiveOrExpression
    :   andExpression ( '^' andExpression {RecordSpecialCharacter('^');})*
    ;

andExpression
    :   equalityExpression ( '&' equalityExpression {RecordSpecialCharacter('&');})*
    ;

equalityExpression
    :   instanceOfExpression ( ('==' {RecordSpecialCharacter('=');RecordSpecialCharacter('=');} | '!=' {RecordSpecialCharacter('!');RecordSpecialCharacter('=');} ) instanceOfExpression )*
    ;

instanceOfExpression
    :   relationalExpression ('instanceof' type {RecordKeyword("instanceof");})?
    ;

relationalExpression
    :   shiftExpression ( relationalOp shiftExpression )*
    ;
    
relationalOp
    :   ('<' '='{RecordSpecialCharacter('<');RecordSpecialCharacter('=');} )=> t1='<' t2='=' 
        { $t1.Line == $t2.Line && 
          $t1.CharPositionInLine + 1 == $t2.CharPositionInLine }?
    |   ('>' '=' {RecordSpecialCharacter('>');RecordSpecialCharacter('=');})=> t1='>' t2='=' 
        { $t1.Line == $t2.Line && 
          $t1.CharPositionInLine + 1 == $t2.CharPositionInLine }?
    |   '<' {RecordSpecialCharacter('<');}
    |   '>' {RecordSpecialCharacter('>');}
    ;

shiftExpression
    :   additiveExpression ( shiftOp additiveExpression )*
    ;

shiftOp
    :   ('<' '<' {RecordSpecialCharacter('<');RecordSpecialCharacter('<');})=> t1='<' t2='<' 
        { $t1.Line == $t2.Line && 
          $t1.CharPositionInLine + 1 == $t2.CharPositionInLine }?
    |   ('>' '>' '>' {RecordSpecialCharacter('>');RecordSpecialCharacter('>');RecordSpecialCharacter('>');})=> t1='>' t2='>' t3='>' 
        { $t1.Line == $t2.Line && 
          $t1.CharPositionInLine + 1 == $t2.CharPositionInLine &&
          $t2.Line == $t3.Line && 
          $t2.CharPositionInLine + 1 == $t3.CharPositionInLine }?
    |   ('>' '>' {RecordSpecialCharacter('>');RecordSpecialCharacter('>');})=> t1='>' t2='>'
        { $t1.Line == $t2.Line && 
          $t1.CharPositionInLine + 1 == $t2.CharPositionInLine }?
    ;


additiveExpression
    :   multiplicativeExpression ( ('+' {RecordSpecialCharacter('+');} | '-' {RecordSpecialCharacter('-');}) multiplicativeExpression )*
    ;

multiplicativeExpression
    :   unaryExpression ( ( '*' {RecordSpecialCharacter('*');} | '/' {RecordSpecialCharacter('/');} | '%' {RecordSpecialCharacter('\%');} ) unaryExpression )*
    ;
    
unaryExpression
    :   '+' unaryExpression {RecordSpecialCharacter('+');}
    |   '-' unaryExpression {RecordSpecialCharacter('-');}
    |   '++' unaryExpression {RecordSpecialCharacter('+');RecordSpecialCharacter('+');}
    |   '--' unaryExpression {RecordSpecialCharacter('-');RecordSpecialCharacter('-');}
    |   unaryExpressionNotPlusMinus
    ;

unaryExpressionNotPlusMinus
    :   '~' unaryExpression {RecordSpecialCharacter('~');}
    |   '!' unaryExpression {RecordSpecialCharacter('!');}
    |   castExpression
    |   primary selector* ('++' {RecordSpecialCharacter('+');RecordSpecialCharacter('+');} | '--' {RecordSpecialCharacter('-');RecordSpecialCharacter('-');} )?
    ;

castExpression
    :  '(' primitiveType ')' unaryExpression { RecordSpecialCharacter('('); RecordSpecialCharacter(')'); }
    |  '(' (type | expression) ')' unaryExpressionNotPlusMinus { RecordSpecialCharacter('('); RecordSpecialCharacter(')'); }
    ;

primary
    :   parExpression
    |   'this' {RecordKeyword("this");} ('.' Identifier {RecordSpecialCharacter('.');RecordUserDefinedIdentifier($Identifier.text);} )* identifierSuffix?  
    |   'super' superSuffix {RecordKeyword("super");}
    |   literal
    |   'new' creator {RecordKeyword("new");}
    |   id1=Identifier {RecordUserDefinedIdentifier($id1.text);} ('.' id2=Identifier {RecordSpecialCharacter('.');RecordUserDefinedIdentifier($id2.text);})* identifierSuffix?
    |   primitiveType ('[' ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');} )* '.' 'class' {RecordSpecialCharacter('.'); RecordKeyword("class");}
    |   'void' '.' 'class' {RecordKeyword("void");RecordSpecialCharacter('.');RecordKeyword("class");}
    ;

identifierSuffix
    :   ('[' ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');} )+ '.' 'class' {RecordSpecialCharacter('.'); RecordKeyword("class");}
    |   ('[' expression ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');})+ // can also be matched by selector, but do here
    |   arguments
    |   '.' 'class' {RecordSpecialCharacter('.'); RecordKeyword("class");}
    |   '.' explicitGenericInvocation {RecordSpecialCharacter('.');}
    |   '.' 'this' {RecordSpecialCharacter('.'); RecordKeyword("this");}
    |   '.' 'super' arguments {RecordSpecialCharacter('.'); RecordKeyword("super");}
    |   '.' 'new' innerCreator {RecordSpecialCharacter('.'); RecordKeyword("new");}
    ;

creator
    :   nonWildcardTypeArguments createdName classCreatorRest
    |   createdName (arrayCreatorRest | classCreatorRest)
    ;

createdName
    :   classOrInterfaceType
    |   primitiveType
    ;
    
innerCreator
    :   nonWildcardTypeArguments? Identifier classCreatorRest {RecordUserDefinedIdentifier($Identifier.text);}
    ;

arrayCreatorRest
    :   '[' {RecordSpecialCharacter('[');}
        (   ']' {RecordSpecialCharacter(']');} ('[' ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');})* arrayInitializer
        |   expression ']' ('[' expression ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');})* ('[' ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');})*
        )
    ;

classCreatorRest
    :   arguments classBody?
    ;
    
explicitGenericInvocation
    :   nonWildcardTypeArguments Identifier arguments {RecordUserDefinedIdentifier($Identifier.text);}
    ;
    
nonWildcardTypeArguments
    :   '<' typeList '>' {RecordSpecialCharacter('<');RecordSpecialCharacter('>');}
    ;
    
selector
    :   '.' Identifier arguments ? {RecordSpecialCharacter('.');RecordUserDefinedIdentifier($Identifier.text);}
    |   '.' 'this' {RecordSpecialCharacter('.');RecordKeyword("this");}
    |   '.' 'super' superSuffix {RecordSpecialCharacter('.');RecordKeyword("super");}
    |   '.' 'new' innerCreator {RecordSpecialCharacter('.');RecordKeyword("new");}
    |   '[' expression ']' {RecordSpecialCharacter('[');RecordSpecialCharacter(']');}
    ;
    
superSuffix
    :   arguments
    |   '.' Identifier arguments ? //{RecordSpecialCharacter('.');RecordUserDefinedIdentifier($Identifier.text);}
    ;

arguments
    :   '(' expressionList? ')' {RecordSpecialCharacter('(');RecordSpecialCharacter(')');}
    ;

// LEXER

HexLiteral : '0' ('x'|'X') HexDigit+ IntegerTypeSuffix? {charCount++;}; 

DecimalLiteral : ('0' | '1'..'9' '0'..'9'*) IntegerTypeSuffix? {charCount++;};

OctalLiteral : '0' ('0'..'7')+ IntegerTypeSuffix? {charCount++;};

fragment
HexDigit : ('0'..'9'|'a'..'f'|'A'..'F') {charCount++;};

fragment
IntegerTypeSuffix : ('l'|'L') {charCount++;};

FloatingPointLiteral
    :   ('0'..'9')+ '.' ('0'..'9')* Exponent? FloatTypeSuffix? {charCount++;}
    |   '.' ('0'..'9')+ Exponent? FloatTypeSuffix? {charCount++;}
    |   ('0'..'9')+ Exponent FloatTypeSuffix? {charCount++;}
    |   ('0'..'9')+ FloatTypeSuffix {charCount++;}
    ;

fragment
Exponent : ('e'|'E') ('+'|'-')? ('0'..'9')+ {charCount++;};

fragment
FloatTypeSuffix : ('f'|'F'|'d'|'D') {charCount++;};

CharacterLiteral
    :   '\'' ( EscapeSequence | ~('\''|'\\') ) '\'' {charCount++;}
    ;

StringLiteral
    :  '"' ( EscapeSequence | ~('\\'|'"') )* '"' {charCount++;}
    ;

fragment
EscapeSequence
    :   '\\' ('b'|'t'|'n'|'f'|'r'|'\"'|'\''|'\\') {charCount++;}
    |   UnicodeEscape {charCount++;}
    |   OctalEscape {charCount++;}
    ;

fragment
OctalEscape
    :   '\\' ('0'..'3') ('0'..'7') ('0'..'7') {charCount++;}
    |   '\\' ('0'..'7') ('0'..'7') {charCount++;}
    |   '\\' ('0'..'7') {charCount++;}
    ;

fragment
UnicodeEscape
    :   '\\' 'u' HexDigit HexDigit HexDigit HexDigit {charCount++;}
    ;

ENUM:   'enum' {if (!enumIsKeyword) $type=Identifier;}
    ;
    
ASSERT
    :   'assert' {if (!assertIsKeyword) $type=Identifier;}
    ;
    
Identifier 
    :   Letter (Letter|JavaIDDigit {charCount++;})*
    ;

/**I found this char range in JavaCC's grammar, but Letter and Digit overlap.
   Still works, but...
 */
fragment
Letter
    :  '\u0024' {charCount++;}|
       '\u0041'..'\u005a' {charCount++;}|
       '\u005f' {charCount++;}|
       '\u0061'..'\u007a' {charCount++;}|
       '\u00c0'..'\u00d6' {charCount++;}|
       '\u00d8'..'\u00f6' {charCount++;}|
       '\u00f8'..'\u00ff' {charCount++;}|
       '\u0100'..'\u1fff' {charCount++;}|
       '\u3040'..'\u318f' {charCount++;}|
       '\u3300'..'\u337f' {charCount++;}|
       '\u3400'..'\u3d2d' {charCount++;}|
       '\u4e00'..'\u9fff' {charCount++;}|
       '\uf900'..'\ufaff' {charCount++;}
    ;

fragment
JavaIDDigit
    :  '\u0030'..'\u0039' {charCount++;}|
       '\u0660'..'\u0669' {charCount++;}|
       '\u06f0'..'\u06f9' {charCount++;}|
       '\u0966'..'\u096f' {charCount++;}|
       '\u09e6'..'\u09ef' {charCount++;}|
       '\u0a66'..'\u0a6f' {charCount++;}|
       '\u0ae6'..'\u0aef' {charCount++;}|
       '\u0b66'..'\u0b6f' {charCount++;}|
       '\u0be7'..'\u0bef' {charCount++;}|
       '\u0c66'..'\u0c6f' {charCount++;}|
       '\u0ce6'..'\u0cef' {charCount++;}|
       '\u0d66'..'\u0d6f' {charCount++;}|
       '\u0e50'..'\u0e59' {charCount++;}|
       '\u0ed0'..'\u0ed9' {charCount++;}|
       '\u1040'..'\u1049' {charCount++;}
   ;
	

WS  :  (' '|'\r'|'\t'|'\u000C'|'\n') {whiteSpaceCount++; $channel=Hidden;}
    ;
    
Comments
	: ('/*' ( options {greedy=false;} : . )* '*/') => COMMENT {commentCharCount = commentCharCount + 4; $channel=Hidden;}
	| ('//' ((~('\n'|'\r')))* '\r'? '\n') => LINE_COMMENT {commentCharCount = commentCharCount + 2; $channel=Hidden;}
	| EOF
	;
	
fragment
COMMENT
    :   '/*' ( options {greedy=false;} : . )* '*/' //{$channel=Hidden; }
    ;

fragment
LINE_COMMENT
    : '//' ~('\n'|'\r')* '\r'? '\n' //{$channel=Hidden;}
    ;

